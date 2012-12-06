using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;
using System.ComponentModel.Composition;
using Clide;

namespace IntegrationPackage
{
    [Export(typeof(IWpfTextViewCreationListener))]
    [ContentType("text")]
    [TextViewRole(PredefinedTextViewRoles.Document)]
    internal sealed class TextAdornment1Factory : IWpfTextViewCreationListener
    {
        [Import]
        public IDevEnv DevEnv { get; set; }

        public void TextViewCreated(IWpfTextView textView)
        {
        }
    }
}
