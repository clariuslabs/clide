namespace Clide
{
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

		public void Show ()
		{
			if (!IsVisible)
				IsVisible = true;
		}

		public void Close ()
		{
		}
	}

	public class FakeSolution : FakeSolutionExplorerNode, ISolutionNode
	{
		public FakeSolution ()
		{
			IsOpen = true;
			OwningSolution = this;
		}

		public virtual IProjectNode ActiveProject { get; set; }
		public virtual bool IsOpen { get; set; }
		public virtual string PhysicalPath { get; set; }
		public virtual IEnumerable<ISolutionExplorerNode> SelectedNodes { get; set; }

		public virtual void Close (bool saveFirst = true)
		{
			IsOpen = false;
		}

		public virtual void Create (string solutionFile)
		{
			IsOpen = true;
		}

		public virtual void Open (string solutionFile)
		{
			if (IsOpen)
				Close ();

			IsOpen = true;
			Text = Name = Path.GetFileNameWithoutExtension (solutionFile);
			PhysicalPath = solutionFile;
		}

		public virtual void Save ()
		{
		}

		public virtual void SaveAs (string solutionFile)
		{
		}

		public virtual ISolutionFolderNode CreateSolutionFolder (string name)
		{
			var folder = new FakeSolutionFolder(name);
			Nodes.Add (folder);
			return folder;
		}

		public override SolutionNodeKind Kind { get { return SolutionNodeKind.Solution; } }

		public override bool Accept (ISolutionVisitor visitor)
		{
			return SolutionVisitable.Accept (this, visitor);
		}
	}

	public class FakeSolutionFolder : FakeSolutionExplorerNode, ISolutionFolderNode
	{
		public FakeSolutionFolder (string name)
		{
			Text = Name = name;
		}

		public override SolutionNodeKind Kind { get { return SolutionNodeKind.SolutionFolder; } }

		public override bool Accept (ISolutionVisitor visitor)
		{
			return SolutionVisitable.Accept (this, visitor);
		}

		public virtual ISolutionFolderNode CreateSolutionFolder (string name)
		{
			var folder = new FakeSolutionFolder(name);
			Nodes.Add (folder);
			return folder;
		}
	}

	public class FakeSolutionItem : FakeSolutionExplorerNode, ISolutionItemNode
	{
		public FakeSolutionItem (string name)
		{
			Text = Name = name;
		}

		public override SolutionNodeKind Kind { get { return SolutionNodeKind.SolutionItem; } }

		public override bool Accept (ISolutionVisitor visitor)
		{
			return SolutionVisitable.Accept (this, visitor);
		}

		public virtual ISolutionFolderNode OwningSolutionFolder { get { return Parent as ISolutionFolderNode; } }

		public virtual string LogicalPath
		{
			get { return this.RelativePathTo (OwningSolution); }
		}

		public virtual string PhysicalPath { get; set; }
	}

	public class FakeProject : FakeSolutionExplorerNode, IProjectNode
	{
		ExpandoObject properties = new ExpandoObject();
		ExpandoObject userProperties = new ExpandoObject();
		ConcurrentDictionary<string, ExpandoObject> propertiesFor = new ConcurrentDictionary<string, ExpandoObject>();
		ConcurrentDictionary<string, ExpandoObject> userPropertiesFor = new ConcurrentDictionary<string, ExpandoObject>();

		public FakeProject (string name)
		{
			Text = Name = name;
		}

		public override SolutionNodeKind Kind { get { return SolutionNodeKind.Project; } }

		public override bool Accept (ISolutionVisitor visitor)
		{
			return SolutionVisitable.Accept (this, visitor);
		}

		public virtual IFolderNode CreateFolder (string name)
		{
			var folder = new FakeFolder(name);
			Nodes.Add (folder);
			return folder;
		}

		public virtual void AddReference(IProjectNode referencedProject)
		{
		}

		public virtual string PhysicalPath { get; set; }

		public virtual void Save ()
		{
		}

		public virtual dynamic Properties { get { return properties; } }

		public virtual dynamic PropertiesFor (string configurationName)
		{
			return propertiesFor.GetOrAdd (configurationName, _ => new ExpandoObject ());
		}

		public virtual dynamic UserProperties { get { return userProperties; } }

		public IProjectConfiguration Configuration => throw new NotImplementedException();

		public virtual dynamic UserPropertiesFor (string configurationName)
		{
			return userPropertiesFor.GetOrAdd (configurationName, _ => new ExpandoObject ());
		}

		public bool Supports(string capabilities) => true;

		public bool Supports(KnownCapabilities capabilities) => true;
	}

	public class FakeFolder : FakeSolutionExplorerNode, IFolderNode
	{
		public FakeFolder (string name)
		{
			Text = Name = name;
		}

		public override SolutionNodeKind Kind { get { return SolutionNodeKind.Folder; } }

		public override bool Accept (ISolutionVisitor visitor)
		{
			return SolutionVisitable.Accept (this, visitor);
		}

		public virtual IProjectNode OwningProject
		{
			get { return this.Ancestors ().OfType<IProjectNode> ().FirstOrDefault (); }
		}

		public virtual IFolderNode CreateFolder (string name)
		{
			var folder = new FakeFolder(name);
			Nodes.Add (folder);
			return folder;
		}
	}

	public class FakeItem : FakeSolutionExplorerNode, IItemNode
	{
		public FakeItem (string name)
		{
			Text = Name = name;
			Properties = new ExpandoObject ();
		}

		public override SolutionNodeKind Kind { get { return SolutionNodeKind.Item; } }

		public override bool Accept (ISolutionVisitor visitor)
		{
			return SolutionVisitable.Accept (this, visitor);
		}

		public virtual IProjectNode OwningProject
		{
			get { return this.Ancestors ().OfType<IProjectNode> ().FirstOrDefault (); }
		}

		public virtual string LogicalPath
		{
			get { return this.RelativePathTo (OwningProject); }
		}

		public virtual string PhysicalPath { get; set; }

		public virtual dynamic Properties { get; private set; }
	}

	public abstract class FakeSolutionExplorerNode : ISolutionExplorerNode
	{
		ObservableCollection<ISolutionExplorerNode> nodes = new ObservableCollection<ISolutionExplorerNode>();

		protected FakeSolutionExplorerNode ()
		{
			nodes.CollectionChanged += (sender, args) => {
				if (args.Action == NotifyCollectionChangedAction.Add ||
					args.Action == NotifyCollectionChangedAction.Replace) {
					args.NewItems
						.OfType<FakeSolutionExplorerNode> ()
						.AsParallel ()
						.ForAll (node => node.Parent = this);
				} else if (args.Action == NotifyCollectionChangedAction.Remove ||
					  args.Action == NotifyCollectionChangedAction.Replace) {
					args.OldItems
						.OfType<FakeSolutionExplorerNode> ()
						.AsParallel ()
						.ForAll (node => node.Parent = null);
				} else if (args.Action == NotifyCollectionChangedAction.Reset) {
					// Re-set everything.
					nodes.OfType<FakeSolutionExplorerNode> ()
						.AsParallel ()
						.ForAll (node => node.Parent = this);
				}
			};
		}

		public abstract SolutionNodeKind Kind { get; }

		public virtual ISolutionNode OwningSolution { get; set; }

		public virtual IList<ISolutionExplorerNode> Nodes { get { return nodes; } }

		public virtual string Name { get; set; }

		public virtual string Text { get; set; }

		public virtual bool IsHidden { get; set; }

		public virtual bool IsVisible { get; set; }

		public virtual bool IsSelected { get; set; }

		public virtual bool IsExpanded { get; set; }

		public virtual ISolutionExplorerNode Parent { get; set; }

		IEnumerable<ISolutionExplorerNode> ISolutionExplorerNode.Nodes
		{
			get { return Nodes; }
		}

		public abstract bool Accept (ISolutionVisitor visitor);

		public virtual T As<T>() where T : class
		{
			return default (T);
		}

		public virtual void Collapse ()
		{
			IsExpanded = false;
		}

		public virtual void Expand (bool recursively = false)
		{
			IsExpanded = true;
			if (recursively)
				Nodes.AsParallel ().ForAll (n => n.Expand (true));
		}

		public virtual void Select (bool allowMultiple = false)
		{
			IsSelected = true;
		}

		public bool Equals(ISolutionExplorerNode other)
		{
			return false;
		}
	}
}