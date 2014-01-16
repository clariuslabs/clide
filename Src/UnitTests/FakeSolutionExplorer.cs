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

namespace UnitTests
{
    using Clide;
    using Clide.Events;
    using Clide.Sdk.Solution;
    using Clide.Solution;
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
    using System.Dynamic;
    using System.IO;
    using System.Linq;

    public class FakeSolutionExplorer : ISolutionExplorer
    {
        public ISolutionNode Solution { get; set; }

        public IEnumerable<ISolutionExplorerNode> SelectedNodes { get { return Solution.SelectedNodes; } }

        public bool IsVisible { get; set; }

        public void Show()
        {
            if (!IsVisible)
                IsVisible = true;
        }

        public void Close()
        {
        }
    }

    public class FakeSolution : FakeSolutionExplorerNode, ISolutionNode
    {
        public FakeSolution()
        {
            IsOpen = true;
            OwningSolution = this;
        }

        public IProjectNode ActiveProject { get; private set; }

        public virtual bool IsOpen { get; set; }

        public virtual IEnumerable<ISolutionExplorerNode> SelectedNodes { get; set; }

        public virtual void Close(bool saveFirst = true)
        {
            if (IsOpen)
            {
                RaiseSolutionClosing();
                IsOpen = false;
                RaiseSolutionClosed();
            }
        }

        public virtual void Create(string solutionFile)
        {
        }

        public virtual void Open(string solutionFile)
        {
            if (IsOpen)
                Close();

            IsOpen = true;
            DisplayName = Path.GetFileNameWithoutExtension(solutionFile);
            RaiseSolutionOpened();
        }

        public virtual void Save()
        {
        }

        public virtual void SaveAs(string solutionFile)
        {
        }

        public virtual ISolutionFolderNode CreateSolutionFolder(string name)
        {
            var folder = new FakeSolutionFolder(name);
            Nodes.Add(folder);
            return folder;
        }

        public override SolutionNodeKind Kind { get { return SolutionNodeKind.Solution; } }

        public override bool Accept(ISolutionVisitor visitor)
        {
            return SolutionVisitable.Accept(this, visitor);
        }

        public virtual event EventHandler<ProjectEventArgs> ProjectOpened = (sender, args) => { };

        public virtual event EventHandler<ProjectEventArgs> ProjectClosing = (sender, args) => { };

        public virtual event EventHandler SolutionOpened = (sender, args) => { };

        public virtual event EventHandler SolutionClosing = (sender, args) => { };

        public virtual event EventHandler SolutionClosed = (sender, args) => { };

        public virtual void RaiseProjectOpened(ProjectEventArgs args)
        {
            ProjectOpened(this, args);
        }

        public virtual void RaiseProjectClosing(ProjectEventArgs args)
        {
            ProjectClosing(this, args);
        }

        public virtual void RaiseSolutionOpened()
        {
            SolutionOpened(this, EventArgs.Empty);
        }

        public virtual void RaiseSolutionClosing()
        {
            SolutionClosing(this, EventArgs.Empty);
        }

        public virtual void RaiseSolutionClosed()
        {
            SolutionClosed(this, EventArgs.Empty);
        }
    }

    public class FakeSolutionFolder : FakeSolutionExplorerNode, ISolutionFolderNode
    {
        public FakeSolutionFolder(string name)
        {
            DisplayName = name;
        }

        public override SolutionNodeKind Kind { get { return SolutionNodeKind.SolutionFolder; } }

        public override bool Accept(ISolutionVisitor visitor)
        {
            return SolutionVisitable.Accept(this, visitor);
        }

        public virtual ISolutionFolderNode CreateSolutionFolder(string name)
        {
            var folder = new FakeSolutionFolder(name);
            Nodes.Add(folder);
            return folder;
        }
    }

    public class FakeSolutionItem : FakeSolutionExplorerNode, ISolutionItemNode
    {
        public FakeSolutionItem(string name)
        {
            DisplayName = name;
        }

        public override SolutionNodeKind Kind { get { return SolutionNodeKind.SolutionItem; } }

        public override bool Accept(ISolutionVisitor visitor)
        {
            return SolutionVisitable.Accept(this, visitor);
        }

        public virtual ISolutionFolderNode OwningSolutionFolder { get { return Parent as ISolutionFolderNode; } }

        public virtual string PhysicalPath { get; set; }
    }

    public class FakeProject : FakeSolutionExplorerNode, IProjectNode
    {
        private ExpandoObject properties = new ExpandoObject();
        private ConcurrentDictionary<string, ExpandoObject> propertiesFor = new ConcurrentDictionary<string, ExpandoObject>();

        public FakeProject(string name)
        {
            DisplayName = name;
        }

        public override SolutionNodeKind Kind { get { return SolutionNodeKind.Project; } }

        public override bool Accept(ISolutionVisitor visitor)
        {
            return SolutionVisitable.Accept(this, visitor);
        }

        public virtual IProjectConfiguration Configuration { get; set; }

        public virtual IFolderNode CreateFolder(string name)
        {
            var folder = new FakeFolder(name);
            Nodes.Add(folder);
            return folder;
        }

        public virtual string PhysicalPath { get; set; }

        public virtual void Save()
        {
        }

        public virtual dynamic Properties { get { return properties; } }

        public virtual dynamic PropertiesFor(string configurationName)
        {
            return propertiesFor.GetOrAdd(configurationName, _ => new ExpandoObject());
        }
    }

    public class FakeFolder : FakeSolutionExplorerNode, IFolderNode
    {
        public FakeFolder(string name)
        {
            DisplayName = name;
        }

        public override SolutionNodeKind Kind { get { return SolutionNodeKind.Folder; } }

        public override bool Accept(ISolutionVisitor visitor)
        {
            return SolutionVisitable.Accept(this, visitor);
        }

        public virtual IProjectNode OwningProject
        {
            get { return this.Ancestors().OfType<IProjectNode>().FirstOrDefault(); }
        }

        public virtual IFolderNode CreateFolder(string name)
        {
            var folder = new FakeFolder(name);
            Nodes.Add(folder);
            return folder;
        }
    }

    public class FakeItem : FakeSolutionExplorerNode, IItemNode
    {
        public FakeItem(string name)
        {
            DisplayName = name;
            Properties = new ExpandoObject();
        }

        public override SolutionNodeKind Kind { get { return SolutionNodeKind.Item; } }

        public override bool Accept(ISolutionVisitor visitor)
        {
            return SolutionVisitable.Accept(this, visitor);
        }

        public virtual IProjectNode OwningProject
        {
            get { return this.Ancestors().OfType<IProjectNode>().FirstOrDefault(); }
        }

        public virtual string PhysicalPath { get; set; }

        public virtual dynamic Properties { get; private set; }
    }

    public abstract class FakeSolutionExplorerNode : ISolutionExplorerNode
    {
        private ObservableCollection<ISolutionExplorerNode> nodes = new ObservableCollection<ISolutionExplorerNode>();

        protected FakeSolutionExplorerNode()
        {
            nodes.CollectionChanged += (sender, args) =>
            {
                if (args.Action == NotifyCollectionChangedAction.Add ||
                    args.Action == NotifyCollectionChangedAction.Replace)
                {
                    args.NewItems
                        .OfType<FakeSolutionExplorerNode>()
                        .AsParallel()
                        .ForAll(node => node.Parent = this);
                }
                else if (args.Action == NotifyCollectionChangedAction.Remove ||
                    args.Action == NotifyCollectionChangedAction.Replace)
                {
                    args.OldItems
                        .OfType<FakeSolutionExplorerNode>()
                        .AsParallel()
                        .ForAll(node => node.Parent = null);
                }
                else if (args.Action == NotifyCollectionChangedAction.Reset)
                {
                    // Re-set everything.
                    nodes.OfType<FakeSolutionExplorerNode>()
                        .AsParallel()
                        .ForAll(node => node.Parent = this);
                }
            };
        }

        public abstract SolutionNodeKind Kind { get; }

        public virtual ISolutionNode OwningSolution { get; set; }

        public virtual IList<ISolutionExplorerNode> Nodes { get { return nodes; } }

        public virtual string DisplayName { get; set; }

        public virtual bool IsHidden { get; set; }

        public virtual bool IsVisible { get; set; }

        public virtual bool IsSelected { get; set; }

        public virtual bool IsExpanded { get; set; }

        public virtual ITreeNode Parent { get; set; }

        IEnumerable<ISolutionExplorerNode> ISolutionExplorerNode.Nodes
        {
            get { return Nodes; }
        }

        IEnumerable<ITreeNode> ITreeNode.Nodes
        {
            get { return Nodes; }
        }

        public abstract bool Accept(ISolutionVisitor visitor);

        public virtual T As<T>() where T : class
        {
            return default(T);
        }

        public virtual void Collapse()
        {
            IsExpanded = false;
        }

        public virtual void Expand(bool recursively = false)
        {
            IsExpanded = true;
            if (recursively)
                Nodes.AsParallel().ForAll(n => n.Expand(true));
        }

        public virtual void Select(bool allowMultiple = false)
        {
            IsSelected = true;
        }
    }
}