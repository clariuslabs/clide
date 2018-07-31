using Clide;
using EnvDTE;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using VSLangProj;

public static partial class AdapterFacade
{
    /// <summary>
    /// Adapts a <see cref="Solution"/> to an <see cref="ISolutionNode"/>.
    /// </summary>
    /// <returns>The <see cref="ISolutionNode"/> or <see langword="null"/> if conversion is not possible.</returns>
    public static ISolutionNode AsSolutionNode(this Solution solution) =>
        solution.GetServiceLocator().GetExport<IAdapterService>().Adapt(solution).As<ISolutionNode>();

    /// <summary>
    /// Adapts a <see cref="Solution"/> to an <see cref="IVsSolution"/>.
    /// </summary>
    /// <returns>The <see cref="IVsSolution"/> or <see langword="null"/> if conversion is not possible.</returns>
    public static IVsSolution AsVsSolution(this Solution solution) =>
        solution.GetServiceLocator().GetExport<IAdapterService>().Adapt(solution).As<IVsSolution>();


    /// <summary>
    /// Adapts a <see cref="Project"/> to an <see cref="IProjectNode"/>.
    /// </summary>
    /// <returns>The <see cref="IProjectNode"/> or <see langword="null"/> if conversion is not possible.</returns>
    public static IProjectNode AsProjectNode(this Project project) =>
        project.GetServiceLocator().GetExport<IAdapterService>().Adapt(project).As<IProjectNode>();

    /// <summary>
    /// Adapts a <see cref="Project"/> to an <see cref="IVsProject"/>.
    /// </summary>
    /// <returns>The <see cref="IVsProject"/> or <see langword="null"/> if conversion is not possible.</returns>
    public static IVsProject AsVsProject(this Project project) =>
        project.GetServiceLocator().GetExport<IAdapterService>().Adapt(project).As<IVsProject>();

    /// <summary>
    /// Adapts a <see cref="Project"/> to an <see cref="IVsProject"/>.
    /// </summary>
    /// <returns>The <see cref="IVsHierarchy"/> or <see langword="null"/> if conversion is not possible.</returns>
    public static IVsHierarchy AsVsHierarchy(this Project project) =>
        project.GetServiceLocator().GetExport<IAdapterService>().Adapt(project).As<IVsHierarchy>();

    /// <summary>
    /// Adapts a <see cref="Project"/> to an <see cref="IVsProject"/>.
    /// </summary>
    /// <returns>The <see cref="IVsHierarchyItem"/> or <see langword="null"/> if conversion is not possible.</returns>
    public static IVsHierarchyItem AsVsHierarchyItem(this Project project) =>
        project.GetServiceLocator().GetExport<IAdapterService>().Adapt(project).As<IVsHierarchyItem>();

    /// <summary>
    /// Adapts a <see cref="Project"/> to a <see cref="VSProject"/>.
    /// </summary>
    /// <returns>The <see cref="VSProject"/> or <see langword="null"/> if conversion is not possible.</returns>
    public static VSProject AsVsLangProject(this Project project) =>
        project.GetServiceLocator().GetExport<IAdapterService>().Adapt(project).As<VSProject>();

    /// <summary>
    /// Adapts a <see cref="ProjectItem"/> to an <see cref="IItemNode"/>.
    /// </summary>
    /// <returns>The <see cref="IItemNode"/> or <see langword="null"/> if conversion is not possible.</returns>
    public static IItemNode AsItemNode(this ProjectItem item) =>
        item.DTE.GetServiceLocator().GetExport<IAdapterService>().Adapt(item).As<IItemNode>();

    /// <summary>
    /// Adapts a <see cref="ProjectItem"/> to an <see cref="VSProjectItem"/>.
    /// </summary>
    /// <returns>The <see cref="VSProjectItem"/> or <see langword="null"/> if conversion is not possible.</returns>
    public static VSProjectItem AsVsLangItem(this ProjectItem item) =>
        item.DTE.GetServiceLocator().GetExport<IAdapterService>().Adapt(item).As<VSProjectItem>();

    /// <summary>
    /// Adapts a <see cref="ProjectItem"/> to an <see cref="IVsHierarchyItem"/>.
    /// </summary>
    /// <returns>The <see cref="IVsHierarchyItem"/> or <see langword="null"/> if conversion is not possible.</returns>
    public static IVsHierarchyItem AsVsHierarchyItem(this ProjectItem item) =>
        item.ContainingProject.GetServiceLocator().GetExport<IAdapterService>().Adapt(item).As<IVsHierarchyItem>();

    /// <summary>
    /// Adapts a <see cref="Project"/> to a <see cref="VSProject"/>.
    /// </summary>
    /// <returns>The <see cref="VSProject"/> or <see langword="null"/> if conversion is not possible.</returns>
    public static Microsoft.Build.Evaluation.Project AsMsBuildProject(this Project project) =>
        project.GetServiceLocator().GetExport<IAdapterService>().Adapt(project).As<Microsoft.Build.Evaluation.Project>();
}
