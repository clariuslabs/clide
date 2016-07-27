using Clide;

public static partial class AdapterFacade
{
	internal static IProjectItemContainerNode AsContainerNode(this IFolderNode folder) =>
		folder.As<IProjectItemContainerNode>();

	internal static IProjectItemContainerNode AsContainerNode(this IProjectNode project) =>
		project.As<IProjectItemContainerNode>();

	internal static IDeletableNode AsDeletableNode(this IItemNode folder) =>
		folder.As<IDeletableNode>();

	internal static IDeletableNode AsDeletableNode(this IFolderNode folder) =>
		folder.As<IDeletableNode>();

	internal static IRemovableNode AsRemovableNode(this IItemNode folder) =>
		folder.As<IRemovableNode>();

	internal static IRemovableNode AsRemovableNode(this IFolderNode folder) =>
		folder.As<IRemovableNode>();

	internal static IReferenceContainerNode AsReferenceContainerNode(this IReferencesNode references) =>
		references.As<IReferenceContainerNode>();

	internal static IReferenceContainerNode AsReferenceContainerNode(this IProjectNode project) =>
		project.As<IReferenceContainerNode>();
}