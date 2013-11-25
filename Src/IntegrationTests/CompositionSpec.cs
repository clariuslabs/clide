using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VSSDK.Tools.VsIdeTesting;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Language.Intellisense;
using System.ComponentModel.Composition;

namespace Clide
{
    [TestClass]
    public class CompositionSpec : VsHostedSpec
    {
        internal static readonly IAssertion Assert = new Assertion();

        [HostType("VS IDE")]
        [TestMethod]
        public void WhenRetrievingExportedValueOrDefaultForMany_ThenThrowsImportCardinalityException()
        {
            var components = VsIdeTestHostContext.ServiceProvider.GetService<SComponentModel, IComponentModel>();

            // ICompletionSourceProvider has many exports.
            // On VS2010 this throws, but on VS2012+ it succeeds and returns null :S
#if Vs10
            Assert.Throws<ImportCardinalityMismatchException>(() => 
                components.DefaultExportProvider.GetExportedValueOrDefault<ICompletionSourceProvider>());
#endif

#if Vs11 || Vs12
            Assert.Equal<ICompletionSourceProvider>(null, components.DefaultExportProvider.GetExportedValueOrDefault<ICompletionSourceProvider>());
#endif
        }
    }
}
