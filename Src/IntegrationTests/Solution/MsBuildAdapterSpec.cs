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

namespace Clide.Solution
{
    using Microsoft.VisualStudio.Shell.Interop;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using MsBuild = Microsoft.Build.Evaluation;
    using System.Dynamic;

    [TestClass]
    public class MsBuildAdapterSpec : VsHostedSpec
    {
        internal static readonly IAssertion Assert = new Assertion();

        [HostType("VS IDE")]
        [TestMethod]
        public void WhenSolutionIsOpened_ThenCanGetMsBuildProjectProperties()
        {
            base.OpenSolution("SampleSolution\\SampleSolution.sln");

            var explorer = base.ServiceLocator.GetInstance<ISolutionExplorer>();

            var lib = new ITreeNode[] { explorer.Solution }.Traverse(TraverseKind.BreadthFirst, node => node.Nodes)
                .OfType<IProjectNode>()
                .FirstOrDefault(node => node.DisplayName == "ClassLibrary");

            var msb = lib.As<MsBuild.Project>();

            Assert.NotNull(msb);

            //Console.WriteLine("IntermediateOutputPath                                      : {0}", lib.Properties.IntermediateOutputPath);
            //Console.WriteLine("DesignTimeIntermediateOutputPath                                      : {0}", lib.Properties.DesignTimeIntermediateOutputPath);
            //Console.WriteLine("BaseIntermediateOutputPath                                      : {0}", lib.Properties.BaseIntermediateOutputPath);
            //Console.WriteLine("MSBuildProjectDirectory                                      : {0}", lib.Properties.MSBuildProjectDirectory);
            
            //Console.WriteLine("OutputFileName: {0}", lib.Properties.OutputFileName);
            //Console.WriteLine("OutputPath: {0}", lib.Properties.OutputPath);
            //Console.WriteLine("TargetDir                                      : {0}", lib.Properties.TargetDir);
            //Console.WriteLine("TargetedFrameworkDir                           : {0}", lib.Properties.TargetedFrameworkDir);
            //Console.WriteLine("TargetedRuntimeVersion                         : {0}", lib.Properties.TargetedRuntimeVersion);
            //Console.WriteLine("TargetedSDKArchitecture                        : {0}", lib.Properties.TargetedSDKArchitecture);
            //Console.WriteLine("TargetedSDKConfiguration                       : {0}", lib.Properties.TargetedSDKConfiguration);
            //Console.WriteLine("TargetExt                                      : {0}", lib.Properties.TargetExt);
            //Console.WriteLine("TargetFileName                                 : {0}", lib.Properties.TargetFileName);
            //Console.WriteLine("TargetFramework                                : {0}", lib.Properties.TargetFramework);
            //Console.WriteLine("TargetFrameworkAsMSBuildRuntime                : {0}", lib.Properties.TargetFrameworkAsMSBuildRuntime);
            //Console.WriteLine("TargetFrameworkIdentifier                      : {0}", lib.Properties.TargetFrameworkIdentifier);
            //Console.WriteLine("TargetFrameworkMoniker                         : {0}", lib.Properties.TargetFrameworkMoniker);
            //Console.WriteLine("TargetFrameworkMonikerAssemblyAttributesPath   : {0}", lib.Properties.TargetFrameworkMonikerAssemblyAttributesPath);
            //Console.WriteLine("TargetFrameworkMonikerDisplayName              : {0}", lib.Properties.TargetFrameworkMonikerDisplayName);
            //Console.WriteLine("TargetFrameworkSDKToolsDirectory               : {0}", lib.Properties.TargetFrameworkSDKToolsDirectory);
            //Console.WriteLine("TargetFrameworkVersion                         : {0}", lib.Properties.TargetFrameworkVersion);
            //Console.WriteLine("TargetName                                     : {0}", lib.Properties.TargetName);
            //Console.WriteLine("TargetPath                                     : {0}", lib.Properties.TargetPath);
            //Console.WriteLine("TargetPlatformIdentifier                       : {0}", lib.Properties.TargetPlatformIdentifier);
            //Console.WriteLine("TargetPlatformMoniker                          : {0}", lib.Properties.TargetPlatformMoniker);
            //Console.WriteLine("TargetPlatformRegistryBase                     : {0}", lib.Properties.TargetPlatformRegistryBase);
            //Console.WriteLine("TargetPlatformSdkPath                          : {0}", lib.Properties.TargetPlatformSdkPath);
            //Console.WriteLine("TargetPlatformVersion                          : {0}", lib.Properties.TargetPlatformVersion);
            //Console.WriteLine("TargetRuntime                                  : {0}", lib.Properties.TargetRuntime);

            //((DynamicObject)lib.Properties).GetDynamicMemberNames()
            //    .ToList()
            //    .ForEach(prop => Console.WriteLine(prop));
        }
    }
}