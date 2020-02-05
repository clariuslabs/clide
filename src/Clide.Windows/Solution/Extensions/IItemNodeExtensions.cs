namespace Clide
{
    public static partial class SolutionExtensions
    {
        public static void Delete(this IItemNode item) =>
            item.AsDeletableNode().Delete();

        public static void Remove(this IItemNode item) =>
            item.AsRemovableNode().Remove();
    }
}
