using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Clide.Interop;
using Clide.Properties;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Clide
{
	/// <summary>
	/// Default implementation of the root solution node.
	/// </summary>
	public class SolutionNode : SolutionExplorerNode, ISolutionNode
	{
		ISolutionExplorerNodeFactory nodeFactory;

		Lazy<Solution2> dteSolution;
		IVsSolution2 solution;
		IVsSolutionSelection selection;

		/// <summary>
		/// Initializes a new instance of the <see cref="SolutionNode"/> class.
		/// </summary>
		/// <param name="hierarchyNode">The underlying hierarchy represented by this node.</param>
		/// <param name="nodeFactory">The factory for child nodes.</param>
		/// <param name="adapter">The adapter service that implements the smart cast <see cref="ISolutionExplorerNode.As{T}"/>.</param>
		/// <param name="selection">The solution selection service.</param>
		/// <param name="solutionExplorer">The solution explorer window.</param>
		public SolutionNode (
			IServiceProvider services,
			IVsHierarchyItem hierarchyNode,
			ISolutionExplorerNodeFactory nodeFactory,
			IAdapterService adapter,
			IVsSolutionSelection selection,
			Lazy<IVsUIHierarchyWindow> solutionExplorer)
			: base (SolutionNodeKind.Solution, hierarchyNode, nodeFactory, adapter, solutionExplorer)
		{
			this.nodeFactory = nodeFactory;
			dteSolution = new Lazy<Solution2> (() => (Solution2)services.GetService<DTE> ().Solution);
			this.selection = selection;
			solution = (IVsSolution2)hierarchyNode.HierarchyIdentity.Hierarchy;
		}

		/// <summary>
		/// Gets the currently active project (if single), which can be the selected project, or
		/// the project owning the currently selected item or opened designer file.
		/// </summary>
		/// <remarks>
		/// If there are multiple active projects, this property will be null. This can happen
		/// when multiple selection is enabled for items across more than one project
		/// </remarks>
		public virtual IProjectNode ActiveProject
		{
			get
			{
				var selected = selection.GetActiveHierarchy();
				if (selected == null)
					return null;

				return nodeFactory.CreateNode (selected) as IProjectNode;
			}
		}

		/// <summary>
		/// Gets a value indicating whether a solution is open.
		/// </summary>
		public virtual bool IsOpen => solution.GetProperty<bool> (__VSPROPID.VSPROPID_IsSolutionOpen);

		/// <summary>
		/// Gets the physical path of the solution, if it has been saved already.
		/// </summary>
		public virtual string PhysicalPath
		{
			get
			{
				var solutionDir = solution.GetProperty<string>(__VSPROPID.VSPROPID_SolutionDirectory);
				var solutionName = solution.GetProperty<string>(__VSPROPID.VSPROPID_SolutionFileName);
				if (string.IsNullOrEmpty (solutionDir) || string.IsNullOrEmpty (solutionName))
					return "";

				if (!Path.IsPathRooted(solutionName))
					return Path.Combine (solutionDir, solutionName);

				return solutionName;
			}
		}

		/// <summary>
		/// Gets the currently selected nodes in the solution.
		/// </summary>
		public virtual IEnumerable<ISolutionExplorerNode> SelectedNodes => selection.GetSelection ()
			.Select (item => nodeFactory.CreateNode (item));

		/// <summary>
		/// Opens the specified solution file.
		/// </summary>
		public virtual void Open (string solutionFile)
		{
			Guard.NotNullOrEmpty (nameof (solutionFile), solutionFile);

			Close ();

			ErrorHandler.ThrowOnFailure (solution.OpenSolutionFile (0, solutionFile));
		}

		/// <summary>
		/// Creates a new blank solution with the specified solution file location.
		/// </summary>
		public virtual void Create (string solutionFile)
		{
			Guard.NotNullOrEmpty (nameof (solutionFile), solutionFile);
			Guard.IsValid (
				nameof (solutionFile),
				solutionFile,
				s => Path.IsPathRooted (s),
				Strings.SolutionNode.InvalidSolutionFile (solutionFile));

			// Closes existing solution
			Close ();

			if (!Directory.Exists (Path.GetDirectoryName (solutionFile)))
				Directory.CreateDirectory (Path.GetDirectoryName (solutionFile));

			ErrorHandler.ThrowOnFailure (solution.CreateSolution (
				Path.GetDirectoryName (solutionFile),
				Path.GetFileNameWithoutExtension (solutionFile),
				0));

			ErrorHandler.ThrowOnFailure (solution.SaveSolutionElement (
					(uint)__VSSLNSAVEOPTIONS.SLNSAVEOPT_ForceSave,
					null, 0));
		}

		/// <summary>
		/// Closes the solution.
		/// </summary>
		/// <param name="saveFirst">If set to <c>true</c> saves the solution before closing.</param>
		public virtual void Close (bool saveFirst = true)
		{
			if (!solution.GetProperty<bool> (__VSPROPID.VSPROPID_IsSolutionOpen))
				return;

			if (saveFirst)
				Save ();

			ErrorHandler.ThrowOnFailure (solution.CloseSolutionElement (0, null, 0));
		}

		/// <summary>
		/// Saves the current solution.
		/// </summary>
		public virtual void Save ()
		{
			ErrorHandler.ThrowOnFailure (solution.SaveSolutionElement (
				(uint)__VSSLNSAVEOPTIONS.SLNSAVEOPT_SaveIfDirty, null, 0));
		}

		/// <summary>
		/// Saves the current solution to the specified target file.
		/// </summary>
		public virtual void SaveAs (string solutionFile)
		{
			Guard.NotNullOrEmpty (nameof (solutionFile), solutionFile);
			Guard.IsValid (
				nameof (solutionFile),
				solutionFile,
				s => Path.IsPathRooted (s),
				Strings.SolutionNode.InvalidSolutionFile (solutionFile));

			dteSolution.Value.SaveAs (solutionFile);
		}

		/// <summary>
		/// Creates a solution folder under the solution root.
		/// </summary>
		public virtual ISolutionFolderNode CreateSolutionFolder (string name)
		{
			Guard.NotNullOrEmpty (nameof (name), name);

			dteSolution.Value.AddSolutionFolder (name);

			var solutionfolder = HierarchyNode.Children.Single(child =>
				child.GetProperty<string>(VsHierarchyPropID.Name) == name);

			return CreateNode (solutionfolder) as ISolutionFolderNode;
		}

		/// <summary>
		/// Accepts the specified visitor for traversal.
		/// </summary>
		public override bool Accept (ISolutionVisitor visitor) => SolutionVisitable.Accept (this, visitor);

		/// <summary>
		/// Tries to smart-cast this node to the give type.
		/// </summary>
		/// <typeparam name="T">Type to smart-cast to.</typeparam>
		/// <returns>
		/// The casted value or null if it cannot be converted to that type.
		/// </returns>
		/// <exception cref="System.NotImplementedException"></exception>
		public override T As<T> () => Adapter.Adapt (this).As<T> ();
	}
}