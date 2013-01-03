using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;
using System.ComponentModel.Composition;
using Clide;
using System.Windows;
using Microsoft.VisualStudio.Shell;

namespace IntegrationPackage
{
    [ExportMetadata("IsClide", true)]
    [Export(typeof(IWpfTextViewCreationListener))]
    [ContentType("text")]
    [TextViewRole(PredefinedTextViewRoles.Document)]
    public sealed class TextAdornment1Factory : IWpfTextViewCreationListener
    {
        public IDevEnv DevEnv { get; set; }

        public TextAdornment1Factory([Import(typeof(SVsServiceProvider))] IServiceProvider services)
        {
            this.DevEnv = Clide.DevEnv.Get(services);
        }

        public void TextViewCreated(IWpfTextView textView)
        {
            this.DevEnv.MessageBoxService.ShowInformation("Clide Adornment");
        }
    }
}
