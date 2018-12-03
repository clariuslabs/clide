using Clide;
using Microsoft.VisualStudio.Shell.Interop;

public static partial class AdapterFacade
{
    /// <summary>
    /// Adapts a <see cref="IVsHierarchy"/> to an <see cref="IProjectNode"/>.
    /// </summary>
    /// <returns>The <see cref="IProjectNode"/> or <see langword="null"/> if conversion is not possible.</returns>
    public static IProjectNode AsProjectNode(this IVsHierarchy project) =>
            project.GetServiceLocator().GetExport<IAdapterService>().Adapt(project).As<IProjectNode>();
}