#region BSD License
/* 
Copyright (c) 2012, Clarius Consulting
All rights reserved.

Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:

* Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
* Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/
#endregion

namespace Clide
{
    using System;
    using System.Collections.Generic;
    using System.Dynamic;
    using System.Linq;
    using System.Text;
    using System.ComponentModel.Composition;
    using System.Windows.Controls;
    using Microsoft.VisualStudio.Shell;
    using System.Runtime.InteropServices;
    using Microsoft.VisualStudio.Shell.Interop;
    using Clide.Properties;
    using System.Diagnostics;
    using Microsoft.Win32;
    using System.ComponentModel;
    using System.Linq.Expressions;
    using System.Reflection;
    using Microsoft.VisualStudio;
    using Clide.Composition;
    using Microsoft.CSharp.RuntimeBinder;
    using System.Reflection.Emit;
    using System.CodeDom;
    using Microsoft.CSharp;
    using System.CodeDom.Compiler;
    using System.IO;
    using System.Text.RegularExpressions;
    using System.Collections.Concurrent;
    using Clide.Diagnostics;

    [Component(typeof(IOptionsManager))]
    internal class OptionsManager : IOptionsManager
    {
        private static readonly ITracer tracer = Tracer.Get<OptionsManager>();

        private IServiceProvider serviceProvider;
        private IVsShell vsShell;
        private string registryRoot;
        private IEnumerable<Lazy<IOptionsPage>> optionPages;

        public OptionsManager(IServiceProvider serviceProvider, IVsShell vsShell, IEnumerable<Lazy<IOptionsPage>> optionPages)
        {
            this.serviceProvider = serviceProvider;
            this.vsShell = vsShell;
            this.optionPages = optionPages;

            // This is the same as shellPackage.ApplicationRegistryRoot
            this.registryRoot = VSRegistry.RegistryRoot(serviceProvider, __VsLocalRegistryType.RegType_Configuration, false).Name;
        }

        /// <summary>
        /// Adds the page to the manager using the given owning package identifier.
        /// </summary>
        public void AddPage(IOptionsPage page)
        {
            var pageType = page.GetType();
            var categoryName = pageType.ComponentModel().Category ?? "";
            var displayName = pageType.ComponentModel().DisplayName ?? "";

            if (string.IsNullOrEmpty(categoryName))
                throw new ArgumentException(Strings.OptionsManager.PageCategoryRequired(page.GetType()));

            if (string.IsNullOrEmpty(categoryName))
                throw new ArgumentException(Strings.OptionsManager.PageDisplayNameRequired(page.GetType()));

            //TODO: validate attributes on the type, write to registry, etc.
            RegisterOptionsPage(registryRoot, serviceProvider.GetPackageGuidOrThrow(), categoryName, displayName, pageType);

            // Need to load the page into the collection for the owning package.
            AddPageToPackage(page, serviceProvider.AsDynamicReflection(), serviceProvider.GetType());
        }

        public void AddPages()
        {
            foreach (var optionPage in optionPages)
            {
                AddPage(optionPage.Value);
            }
        }

        private void AddPageToPackage(IOptionsPage page, dynamic package, Type packageType)
        {
            try
            {
                var container = default(Container);
                // If we have access to the container field, we can add any kind of page.
                container = (Container)package._pagesAndProfiles;

                if (container == null)
                {
                    // Calling GetDialogPage with a dummy page type will cause the 
                    // container to be initialized and the given page to be cached. This 
                    // is harmless since there is no accompanying registry information 
                    // for a page with this dummy guid, and hence it never shows up
                    // on the UI.
                    package.GetDialogPage(GetDummyPage(packageType));
                    // Get the value again. It would be non-null this time.
                    container = (Container)package._pagesAndProfiles;

                    Debug.Assert(container != null, "Failed to initialize internal container for dialog pages in the package.");
                }

                if (container != null)
                    container.Add(page);
            }
            catch (RuntimeBinderException)
            {
                throw new NotSupportedException(Strings.OptionsManager.Unsupported);
            }
        }

        private void RegisterOptionsPage(string registryRoot, Guid packageGuid, string categoryName, string pageName, Type pageType, int categoryNameId = 0, int pageNameId = 0)
        {
            try
            {
                using (var rootKey = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Default))
                {
                    var vsPath = registryRoot.Substring(@"HKEY_CURRENT_USER\".Length);
                    using (var categoryKey = rootKey.CreateSubKey(vsPath + @"\ToolsOptionsPages\" + categoryName + @"\", RegistryKeyPermissionCheck.ReadWriteSubTree))
                    {
                        if (categoryKey.GetValueNames().Length == 0)
                        {
                            categoryKey.SetValue("", string.Format("#{0}", categoryNameId), RegistryValueKind.String);
                            categoryKey.SetValue("Package", packageGuid.ToString("B"), RegistryValueKind.String);
                        }

                        using (var pageKey = categoryKey.CreateSubKey(pageName))
                        {
                            if (pageKey.GetValueNames().Length == 0)
                            {
                                pageKey.SetValue("", string.Format("#{0}", pageNameId), RegistryValueKind.String);
                                pageKey.SetValue("Package", packageGuid.ToString("B"), RegistryValueKind.String);
                                pageKey.SetValue("Page", pageType.GUID.ToString("B"), RegistryValueKind.String);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                tracer.Error(ex, Strings.OptionsManager.FailedToRegisterPage(pageType));
            }
        }

        private static ConcurrentDictionary<AssemblyName, Type> dummyPages = new ConcurrentDictionary<AssemblyName, Type>();
        private static Regex shellName = new Regex(@"Microsoft.VisualStudio.Shell.\d\d\.\d");

        private static Type GetDummyPage(Type packageType)
        {
            var shellRef = packageType.Assembly.GetReferencedAssemblies().FirstOrDefault(a => shellName.IsMatch(a.Name));
            if (shellRef == null)
            {
                tracer.Critical(Strings.OptionsManager.ShellReferenceNotFound(packageType));
                throw new ArgumentException(Strings.OptionsManager.ShellReferenceNotFound(packageType));
            }

            return dummyPages.GetOrAdd(shellRef, name => GeneratePageType(name));
        }

        private static Type GeneratePageType(AssemblyName shellAssemblyName)
        {
            var source = @"
namespace Clide.Dynamic
{
    using System.Runtime.InteropServices;
    using Microsoft.VisualStudio.Shell;
    
    [Guid(""D4388FC3-B7AD-44B0-A40D-F8578BD48CE9"")]
    [ComVisible(true)]
    public class DummyPage : DialogPage
    {
    }
}";

            var options = new CompilerParameters
            {
                GenerateInMemory = true,
                IncludeDebugInformation = false,
                TreatWarningsAsErrors = false,
            };

            options.ReferencedAssemblies.Add(AppDomain.CurrentDomain.GetAssemblies().First(a => a.GetName().Name == "System").ManifestModule.FullyQualifiedName);
            options.ReferencedAssemblies.Add(AppDomain.CurrentDomain.GetAssemblies().First(a => a.GetName().Name == "System.Windows.Forms").ManifestModule.FullyQualifiedName);

            var shellAssembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.GetName().FullName == shellAssemblyName.FullName);
            if (shellAssembly == null)
                shellAssembly = Assembly.Load(shellAssemblyName);

            options.ReferencedAssemblies.Add(shellAssembly.ManifestModule.FullyQualifiedName);

            var results = new CSharpCodeProvider().CompileAssemblyFromSource(options, source);

            Debug.Assert(!results.Errors.HasErrors, "Failed to compile placeholder dialog page");

            var type = results.CompiledAssembly.GetExportedTypes().First();

            return type;
        }

        private static bool FileExists(string fileName)
        {
            try
            {
                return File.Exists(fileName);
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}