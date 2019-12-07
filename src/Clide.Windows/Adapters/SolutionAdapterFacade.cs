using System.ComponentModel;
using Clide;
using EnvDTE;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using VSLangProj;

/// <summary>
/// Facades provide easy discoverability of available adapters, 
/// while still leveraging the <see cref="IServiceLocator"/> and 
/// <see cref="IServiceLocatorProvider"/> exported for the given 
/// target object.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static partial class Adapters
{
    #region DTE

    /// <summary>
    /// Adapts a <see cref="ISolutionNode"/> to a DTE <see cref="Solution"/>.
    /// </summary>
    /// <returns>The DTE <see cref="Solution"/> or <see langword="null"/> if conversion is not possible.</returns>
    public static Solution AsSolution(this ISolutionNode solution) => solution.As<EnvDTE.Solution>();

    /// <summary>
    /// Adapts a <see cref="IItemNode"/> to a <see cref="ProjectItem"/>.
    /// </summary>
    /// <returns>The <see cref="ProjectItem"/> or <see langword="null"/> if conversion is not possible.</returns>
    public static ProjectItem AsProjectItem(this IItemNode item) => item.As<ProjectItem>();


    #endregion

    #region IVsHierarchyItem

    /// <summary>
    /// Adapts a <see cref="ISolutionNode"/> to an <see cref="IVsHierarchyItem"/>.
    /// </summary>
    /// <returns>The <see cref="IVsSolution"/> or <see langword="null"/> if conversion is not possible.</returns>
    public static IVsHierarchyItem AsVsHierarchyItem(this ISolutionNode solution) => solution.As<IVsHierarchyItem>();

    /// <summary>
    /// Adapts a <see cref="ISolutionFolderNode"/> to an <see cref="IVsHierarchyItem"/>.
    /// </summary>
    /// <returns>The <see cref="IVsHierarchyItem"/> or <see langword="null"/> if conversion is not possible.</returns>
    public static IVsHierarchyItem AsVsHierarchyItem(this ISolutionFolderNode folder) => folder.As<IVsHierarchyItem>();

    /// <summary>
    /// Adapts a <see cref="ISolutionItemNode"/> to an <see cref="IVsHierarchyItem"/>.
    /// </summary>
    /// <returns>The <see cref="IVsHierarchyItem"/> or <see langword="null"/> if conversion is not possible.</returns>
    public static IVsHierarchyItem AsVsHierarchyItem(this ISolutionItemNode item) => item.As<IVsHierarchyItem>();

    /// <summary>
    /// Adapts a <see cref="IProjectNode"/> to an <see cref="IVsHierarchyItem"/>.
    /// </summary>
    /// <returns>The <see cref="IVsHierarchyItem"/> or <see langword="null"/> if conversion is not possible.</returns>
    public static IVsHierarchyItem AsVsHierarchyItem(this IProjectNode project) => project.As<IVsHierarchyItem>();

    /// <summary>
    /// Adapts a <see cref="IFolderNode"/> to an <see cref="IVsHierarchyItem"/>.
    /// </summary>
    /// <returns>The <see cref="IVsHierarchyItem"/> or <see langword="null"/> if conversion is not possible.</returns>
    public static IVsHierarchyItem AsVsHierarchyItem(this IFolderNode folder) => folder.As<IVsHierarchyItem>();

    /// <summary>
    /// Adapts a <see cref="IItemNode"/> to an <see cref="IVsHierarchyItem"/>.
    /// </summary>
    /// <returns>The <see cref="IVsHierarchyItem"/> or <see langword="null"/> if conversion is not possible.</returns>
    public static IVsHierarchyItem AsVsHierarchyItem(this IItemNode item) => item.As<IVsHierarchyItem>();

    /// <summary>
    /// Adapts a <see cref="IReferencesNode"/> to an <see cref="IVsHierarchyItem"/>.
    /// </summary>
    /// <returns>The <see cref="IVsHierarchyItem"/> or <see langword="null"/> if conversion is not possible.</returns>
    public static IVsHierarchyItem AsVsHierarchyItem(this IReferencesNode references) => references.As<IVsHierarchyItem>();

    /// <summary>
    /// Adapts a <see cref="IReferenceNode"/> to an <see cref="IVsHierarchyItem"/>.
    /// </summary>
    /// <returns>The <see cref="IVsHierarchyItem"/> or <see langword="null"/> if conversion is not possible.</returns>
    public static IVsHierarchyItem AsVsHierarchyItem(this IReferenceNode reference) => reference.As<IVsHierarchyItem>();

    #endregion

    #region IVsHierarchy

    /// <summary>
    /// Adapts a <see cref="ISolutionNode"/> to an <see cref="IVsHierarchy"/>.
    /// </summary>
    /// <returns>The <see cref="IVsSolution"/> or <see langword="null"/> if conversion is not possible.</returns>
    public static IVsHierarchy AsVsHierarchy(this ISolutionNode solution) => solution.As<IVsHierarchy>();

    /// <summary>
    /// Adapts a <see cref="ISolutionFolderNode"/> to an <see cref="IVsHierarchy"/>.
    /// </summary>
    /// <returns>The <see cref="IVsHierarchy"/> or <see langword="null"/> if conversion is not possible.</returns>
    public static IVsHierarchy AsVsHierarchy(this ISolutionFolderNode folder) => folder.As<IVsHierarchy>();

    /// <summary>
    /// Adapts a <see cref="ISolutionItemNode"/> to an <see cref="IVsHierarchy"/>.
    /// </summary>
    /// <returns>The <see cref="IVsHierarchy"/> or <see langword="null"/> if conversion is not possible.</returns>
    public static IVsHierarchy AsVsHierarchy(this ISolutionItemNode item) => item.As<IVsHierarchy>();

    /// <summary>
    /// Adapts a <see cref="IProjectNode"/> to an <see cref="IVsHierarchy"/>.
    /// </summary>
    /// <returns>The <see cref="IVsHierarchy"/> or <see langword="null"/> if conversion is not possible.</returns>
    public static IVsHierarchy AsVsHierarchy(this IProjectNode project) => project.As<IVsHierarchy>();

    /// <summary>
    /// Adapts a <see cref="IFolderNode"/> to an <see cref="IVsHierarchy"/>.
    /// </summary>
    /// <returns>The <see cref="IVsHierarchy"/> or <see langword="null"/> if conversion is not possible.</returns>
    public static IVsHierarchy AsVsHierarchy(this IFolderNode folder) => folder.As<IVsHierarchy>();

    /// <summary>
    /// Adapts a <see cref="IItemNode"/> to an <see cref="IVsHierarchy"/>.
    /// </summary>
    /// <returns>The <see cref="IVsHierarchy"/> or <see langword="null"/> if conversion is not possible.</returns>
    public static IVsHierarchy AsVsHierarchy(this IItemNode item) => item.As<IVsHierarchy>();

    /// <summary>
    /// Adapts a <see cref="IReferencesNode"/> to an <see cref="IVsHierarchy"/>.
    /// </summary>
    /// <returns>The <see cref="IVsHierarchy"/> or <see langword="null"/> if conversion is not possible.</returns>
    public static IVsHierarchy AsVsHierarchy(this IReferencesNode references) => references.As<IVsHierarchy>();

    /// <summary>
    /// Adapts a <see cref="IReferenceNode"/> to an <see cref="IVsHierarchy"/>.
    /// </summary>
    /// <returns>The <see cref="IVsHierarchy"/> or <see langword="null"/> if conversion is not possible.</returns>
    public static IVsHierarchy AsVsHierarchy(this IReferenceNode reference) => reference.As<IVsHierarchy>();

    #endregion

    #region IVs*

    /// <summary>
    /// Adapts a <see cref="ISolutionNode"/> to an <see cref="IVsSolution"/>.
    /// </summary>
    /// <returns>The <see cref="IVsSolution"/> or <see langword="null"/> if conversion is not possible.</returns>
    public static IVsSolution AsVsSolution(this ISolutionNode solution) => solution.As<IVsSolution>();

    /// <summary>
    /// Adapts a <see cref="IProjectNode"/> to an <see cref="IVsProject"/>.
    /// </summary>
    /// <returns>The <see cref="IVsProject"/> or <see langword="null"/> if conversion is not possible.</returns>
    public static IVsProject AsVsProject(this IProjectNode project) => project.As<IVsProject>();

    /// <summary>
    /// Adapts a <see cref="IProjectNode"/> to an <see cref="IVsBuildPropertyStorage"/>.
    /// </summary>
    /// <returns>The <see cref="IVsBuildPropertyStorage"/> or <see langword="null"/> if conversion is not possible.</returns>
    public static IVsBuildPropertyStorage AsVsBuildPropertyStorage(this IProjectNode project) => project.As<IVsBuildPropertyStorage>();

    #endregion

    #region VSLang

    /// <summary>
    /// Adapts a <see cref="IProjectNode"/> to a <see cref="VSProject"/>.
    /// </summary>
    /// <returns>The <see cref="VSProject"/> or <see langword="null"/> if conversion is not possible.</returns>
    public static VSProject AsVsLangProject(this IProjectNode project) => project.As<VSProject>();

    /// <summary>
    /// Adapts a <see cref="IItemNode"/> to an <see cref="VSProjectItem"/>.
    /// </summary>
    /// <returns>The <see cref="VSProjectItem"/> or <see langword="null"/> if conversion is not possible.</returns>
    public static VSProjectItem AsVsLangProjectItem(this IItemNode item) => item.As<VSProjectItem>();

    /// <summary>
    /// Adapts a <see cref="IReferencesNode"/> to a VsLang <see cref="References"/>.
    /// </summary>
    /// <returns>The <see cref="References"/> or <see langword="null"/> if conversion is not possible.</returns>
    public static References AsReferences(this IReferencesNode references) => references.As<References>();

    /// <summary>
    /// Adapts a <see cref="IReferenceNode"/> to a VsLang <see cref="Reference"/>.
    /// </summary>
    /// <returns>The <see cref="Reference"/> or <see langword="null"/> if conversion is not possible.</returns>
    public static Reference AsReference(this IReferenceNode reference) => reference.As<Reference>();

    #endregion



    /// <summary>
    /// Adapts a <see cref="IProjectNode"/> to a <see cref="Microsoft.Build.Evaluation.Project"/>.
    /// </summary>
    /// <returns>The <see cref="Microsoft.Build.Evaluation.Project"/> or <see langword="null"/> if conversion is not possible.</returns>
    public static Microsoft.Build.Evaluation.Project AsMsBuildProject(this IProjectNode project) =>
        project.As<Microsoft.Build.Evaluation.Project>();
}