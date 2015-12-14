using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;
using System;
using System.ComponentModel.Composition;
using System.Diagnostics;

namespace IntegrationPackage
{
    [ExportMetadata("IsClide", true)]
    [Export(typeof(IWpfTextViewCreationListener))]
    [ContentType("text")]
    [TextViewRole(PredefinedTextViewRoles.Document)]
    public sealed class TextAdornment1Factory : IWpfTextViewCreationListener
    {
        private IServiceProvider services;

        [ImportingConstructor]
        public TextAdornment1Factory([Import(Constants.ShellContractName)] IServiceProvider services)
        {
            this.services = services;
        }

        public void TextViewCreated(IWpfTextView textView)
        {
            Debug.Assert(Clide.DevEnv.Get(services) != null);
        }
    }
}
