﻿using Xunit;

namespace Clide.Solution.Item
{
    [Trait("LongRunning", "true")]
    [Collection("OpenSolution")]
    public class ItemNodeFactorySharedSpec : NodeFactorySpec<ItemNodeFactory>
    {
        [InlineData("Shared\\CppShared\\SharedSource.cpp", "")]
        [InlineData("Shared\\CsShared\\SharedClass1.cs", "")]
        [InlineData("Shared\\VbShared\\SharedClass1.vb", "14.0")]
        [VsixTheory(MinimumVisualStudioVersion = VisualStudioVersion.VS2013)]
        public override void when_item_is_supported_then_factory_supports_it(string relativePath, string minimumVersion)
        {
            base.when_item_is_supported_then_factory_supports_it(relativePath, minimumVersion);
        }

        [InlineData("Shared\\CppShared\\CppSharedFolder", "")]
        [InlineData("Shared\\CsShared\\CsSharedFolder", "")]
        [InlineData("Shared\\VbShared\\VbSharedFolder", "14.0")]
        [VsixTheory(MinimumVisualStudioVersion = VisualStudioVersion.VS2013)]
        public override void when_item_is_not_supported_then_factory_returns_false_and_create_returns_null(string relativePath, string minimumVersion)
        {
            base.when_item_is_not_supported_then_factory_returns_false_and_create_returns_null(relativePath, minimumVersion);
        }
    }

    [Trait("LongRunning", "true")]
    [Collection("OpenSolution11")]
    public class ItemNodeFactorySpec : NodeFactorySpec<ItemNodeFactory>
    {
        [InlineData("Native\\CppLibrary\\ReadMe.txt")]
        [InlineData("Native\\NsLibrary\\Class1.cs")]
        [InlineData("Native\\NsLibrary\\CsFolder\\CsTextInFolder.txt")]
        [InlineData("Native\\CsLibrary\\Class1.cs")]
        [InlineData("Native\\VbLibrary\\Class1.vb")]
        [InlineData("Native\\FsLibrary\\Library1.fs")]
        [InlineData("PclLibrary\\Class1.cs")]
        [VsixTheory]
        public override void when_item_is_supported_then_factory_supports_it(string relativePath)
        {
            base.when_item_is_supported_then_factory_supports_it(relativePath);
        }

        [InlineData("Native\\CppLibrary\\References")]
        [InlineData("Native\\CppLibrary\\References\\System")]
        [InlineData("Native\\VbLibrary\\References")]
        [InlineData("Native\\VbLibrary\\References\\System")]
        [VsixTheory(MinimumVisualStudioVersion = VisualStudioVersion.VS2015)]
        public void when_2015_item_is_not_supported_then_factory_returns_false_and_create_returns_null(string relativePath)
        {
            when_item_is_not_supported_then_factory_returns_false_and_create_returns_null(relativePath);
        }

        [InlineData("")]
        [InlineData("Native")]
        [InlineData("Native\\CppLibrary")]
        [InlineData("Native\\CsLibrary")]
        [InlineData("Native\\VbLibrary")]
        [InlineData("Native\\FsLibrary")]
        [InlineData("PclLibrary")]
        [InlineData("Native\\CppLibrary\\CppFolder")]
        [InlineData("Native\\CsLibrary\\CsFolder")]
        [InlineData("Native\\VbLibrary\\VbFolder")]
        [InlineData("Native\\FsLibrary\\FsFolder")]
        [InlineData("Native\\CppLibrary\\External Dependencies")]
        [InlineData("Native\\CsLibrary\\References")]
        [InlineData("Native\\CsLibrary\\References\\System")]
        [InlineData("Native\\FsLibrary\\References")]
        [InlineData("Native\\FsLibrary\\References\\System")]
        [InlineData("PclLibrary\\References")]
        [InlineData("PclLibrary\\References\\.NET")]
        [InlineData("Solution Items")]
        [InlineData("Solution Items\\SolutionItem.txt")]
        [VsixTheory]
        public override void when_item_is_not_supported_then_factory_returns_false_and_create_returns_null(string relativePath)
        {
            base.when_item_is_not_supported_then_factory_returns_false_and_create_returns_null(relativePath);
        }
    }
}
