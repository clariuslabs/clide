using Xunit;

namespace Clide.Solution.References
{
    [Trait("LongRunning", "true")]
    [Collection("OpenSolution11")]
    public class ReferencesNodeFactorySpec : NodeFactorySpec<ReferencesNodeFactory>
    {
        [InlineData("Native\\CppLibrary\\References")]
        [InlineData("Native\\VbLibrary\\References")]
        [VsixTheory(MinimumVisualStudioVersion = VisualStudioVersion.VS2015)]
        public void when_2015_item_is_supported_then_factory_supports_it(string relativePath)
        {
            when_item_is_supported_then_factory_supports_it(relativePath);
        }

        [InlineData("Native\\CsLibrary\\References")]
        [InlineData("PclLibrary\\References")]
        [VsixTheory]
        public override void when_item_is_supported_then_factory_supports_it(string relativePath)
        {
            base.when_item_is_supported_then_factory_supports_it(relativePath);
        }

        [InlineData("Native\\CppLibrary\\References\\System")]
        [InlineData("Native\\VbLibrary\\References\\System")]
        [VsixTheory(MinimumVisualStudioVersion = VisualStudioVersion.VS2015)]
        public void when_2015_item_is_not_supported_then_factory_returns_false_and_create_returns_null(string relativePath)
        {
            when_item_is_not_supported_then_factory_returns_false_and_create_returns_null(relativePath);
        }

        // TODO: we need to support F# if possible
        [InlineData("Native\\FsLibrary\\References")]
        [InlineData("")]
        [InlineData("Native")]
        [InlineData("Native\\CppLibrary")]
        [InlineData("Native\\CppLibrary\\CppFolder")]
        [InlineData("Native\\CppLibrary\\External Dependencies")]
        [InlineData("Native\\CsLibrary")]
        [InlineData("Native\\VbLibrary")]
        [InlineData("Native\\FsLibrary")]
        [InlineData("PclLibrary")]
        [InlineData("Native\\FsLibrary\\References\\System")]
        [InlineData("Native\\CsLibrary\\References\\System")]
        [InlineData("Solution Items")]
        [InlineData("Solution Items\\SolutionItem.txt")]
        [InlineData("Native\\CppLibrary\\ReadMe.txt")]
        [InlineData("Native\\CsLibrary\\Class1.cs")]
        [InlineData("Native\\VbLibrary\\Class1.vb")]
        [InlineData("Native\\FsLibrary\\Library1.fs")]
        [InlineData("PclLibrary\\Class1.cs")]
        [VsixTheory]
        public override void when_item_is_not_supported_then_factory_returns_false_and_create_returns_null(string relativePath)
        {
            base.when_item_is_not_supported_then_factory_returns_false_and_create_returns_null(relativePath);
        }
    }
}