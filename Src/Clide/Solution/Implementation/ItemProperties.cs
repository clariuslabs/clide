#region BSD License
/* 
Copyright (c) 2012, Clarius Consulting
All rights reserved.

Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:

* Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
* Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/
#endregion

namespace Clide.Solution.Implementation
{
    using Clide.Diagnostics;
    using Clide.Sdk.Solution;
    using EnvDTE;
    using Microsoft.VisualStudio;
    using Microsoft.VisualStudio.Shell.Interop;
    using System;
    using System.Collections.Generic;
    using System.Dynamic;
    using System.Linq;

    internal class ItemProperties : DynamicObject
    {
        private static readonly ITracer tracer = Tracer.Get<ItemProperties>();

        IVsSolutionHierarchyNode node;
        ProjectItem item;
        private IVsBuildPropertyStorage msBuild;
        private string debugString;

        public ItemProperties(ItemNode item)
        {
            this.node = item.HierarchyNode;
            this.item = item.HierarchyNode.ExtensibilityObject as ProjectItem;
            this.msBuild = item.HierarchyNode.VsHierarchy as IVsBuildPropertyStorage;
            if (System.Diagnostics.Debugger.IsAttached)
                debugString = string.Join(Environment.NewLine, GetDynamicMemberNames()
                    .Select(name => name + "=" + GetValue(name)));
        }

        public override IEnumerable<string> GetDynamicMemberNames()
        {
            return GetPropertyNames();
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            return SetValue(binder.Name, value);
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            result = GetValue(binder.Name);
            return true;
        }

        public object GetValue(string name)
        {
            string value = null;

            if (this.item != null)
            {
                Property property;
                try
                {
                    property = this.item.Properties.Item(name);
                }
                catch (ArgumentException)
                {
                    property = null;
                }

                if (property != null)
                {
                    return property.Value;
                }
            }

            if (this.msBuild != null)
            {
                this.msBuild.GetItemAttribute(this.node.ItemId, name, out value);
            }

            return value;
        }

        public bool SetValue(string name, object value)
        {
            if (value == null)
                throw new NotSupportedException("Cannot set null value for item properties.");

            if (this.item != null)
            {
                Property property;
                try
                {
                    property = this.item.Properties.Item(name);
                }
                catch (ArgumentException)
                {
                    property = null;
                }

                if (property != null)
                {
                    try
                    {
                        property.Value = value;
                        return true;
                    }
                    catch
                    {
                        return false;
                    }
                }
            }

            // Fallback to MSBuild item properties.
            if (this.msBuild != null)
            {
                return ErrorHandler.Succeeded(
                    this.msBuild.SetItemAttribute(this.node.ItemId, name, value.ToString()));
            }

            return false;
        }

        private IEnumerable<string> GetPropertyNames()
        {
            try
            {
                return ((ProjectItem)this.node.ExtensibilityObject)
                    .Properties
                    .Cast<Property>()
                    .Select(prop => prop.Name);
            }
            catch
            {
                return Enumerable.Empty<string>();
            }
        }

        public override string ToString()
        {
            return this.debugString ?? base.ToString();
        }
    }
}
