using System;
using EnvDTE;
using Microsoft.VisualStudio.Shell.Interop;

namespace Clide
{
	interface IServiceLocatorProvider
	{
		IServiceLocator GetServiceLocator (IServiceProvider services);
		IServiceLocator GetServiceLocator (DTE dte);
		IServiceLocator GetServiceLocator (Project project);
		IServiceLocator GetServiceLocator (IVsHierarchy hierarchy);
		IServiceLocator GetServiceLocator (IVsProject project);
	}
}
