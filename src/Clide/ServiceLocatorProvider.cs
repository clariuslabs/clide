using System;
using System.ComponentModel.Composition;
using EnvDTE;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Ole = Microsoft.VisualStudio.OLE.Interop;

namespace Clide
{
    /// <summary>
    /// Implements service locator retrieval as well as provides the 
    /// global service locator export.
    /// </summary>
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(IServiceLocatorProvider))]
    internal class ServiceLocatorProvider : IServiceLocatorProvider
    {
        [ImportingConstructor]
        public ServiceLocatorProvider([Import(typeof(SVsServiceProvider))] IServiceProvider serviceProvider)
        {
            ServiceLocator = new ServiceLocatorImpl(serviceProvider);
        }

        [Export]
        public IServiceLocator ServiceLocator { get; }

        public IServiceLocator GetServiceLocator(IServiceProvider services)
        {
            Guard.NotNull(nameof(services), services);

            return new ServiceLocatorImpl(services);
        }

        public IServiceLocator GetServiceLocator(DTE dte)
        {
            Guard.NotNull(nameof(dte), dte);

            return new ServiceLocatorImpl(new OleServiceProvider(dte));
        }

        public IServiceLocator GetServiceLocator(Project project)
        {
            Guard.NotNull(nameof(project), project);

            return GetServiceLocator(project.DTE);
        }

        public IServiceLocator GetServiceLocator(IVsProject project)
        {
            Guard.NotNull(nameof(project), project);

            return GetServiceLocator((IVsHierarchy)project);
        }

        public IServiceLocator GetServiceLocator(IVsHierarchy hierarchy)
        {
            Guard.NotNull(nameof(hierarchy), hierarchy);

            IServiceProvider services;
            Ole.IServiceProvider site;
            if (ErrorHandler.Failed(hierarchy.GetSite(out site)) || site == null)
                services = Microsoft.VisualStudio.Shell.ServiceProvider.GlobalProvider;
            else
                services = new OleServiceProvider(site);

            return new ServiceLocatorImpl(services);
        }
    }
}