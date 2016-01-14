using System;
using System.ComponentModel.Composition;
using EnvDTE;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Ole = Microsoft.VisualStudio.OLE.Interop;

namespace Clide
{
	[Export (typeof (IServiceLocatorProvider))]
	internal class ServiceLocatorProvider : IServiceLocatorProvider
	{
		public IServiceLocator GetServiceLocator (IServiceProvider services)
		{
			Guard.NotNull ("services", services);

			return new ServiceLocator (services);
		}

		public IServiceLocator GetServiceLocator (DTE dte)
		{
			Guard.NotNull ("dte", dte);

			return new ServiceLocator (new ServiceProvider ((Ole.IServiceProvider)dte));
		}

		public IServiceLocator GetServiceLocator (Project project)
		{
			Guard.NotNull ("project", project);

			return GetServiceLocator (project.DTE);
		}

		public IServiceLocator GetServiceLocator (IVsProject project)
		{
			Guard.NotNull ("project", project);

			return GetServiceLocator ((IVsHierarchy)project);
		}

		public IServiceLocator GetServiceLocator (IVsHierarchy hierarchy)
		{
			Guard.NotNull ("hierarchy", hierarchy);

			IServiceProvider services;
			Ole.IServiceProvider site;
			if (ErrorHandler.Failed (hierarchy.GetSite (out site)))
				services = ServiceProvider.GlobalProvider;
			else
				services = new ServiceProvider (site);

			return new ServiceLocator (services);
		}
	}
}