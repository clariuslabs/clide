using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Xunit.Abstractions;

namespace Clide
{
	public static class Extensions
	{
        public const string ProjectType_SolutionFolder = "{66A26720-8FB5-11D2-AA7E-00C04F688DDE}";
        public const string ProjectItemType_SolutionFolder = "{66A26722-8FB5-11D2-AA7E-00C04F688DDE}";

        public static EnvDTE.Project[] AllProjects(this EnvDTE.Solution solution)
        {
            var results = new List<EnvDTE.Project>();
            var queue = new Queue<EnvDTE.Project>(solution.Projects.OfType<EnvDTE.Project>());

            while (queue.Count > 0)
            {
                var project = queue.Dequeue();
                results.Add(project);
                if (project.ProjectItems != null)
                {
                    foreach (EnvDTE.ProjectItem item in project.ProjectItems)
                    {
                        if (item.Kind == ProjectType_SolutionFolder || item.Kind == ProjectItemType_SolutionFolder)
                        {
                            if (item.SubProject != null)
                                queue.Enqueue(item.SubProject);
                        }
                    }
                }
            }

            return results.ToArray();
        }

        public static IVsHierarchyItem NavigateToItem (this IVsHierarchyItem item, string relativePath)
		{
			IVsHierarchyItem child = null;
			var paths = relativePath.Split(new [] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries);
			SpinWait.SpinUntil (() => 
				(child = NavigateToItem (item, paths, 0)) != null, 
				TimeSpan.FromSeconds (2));

			return child;
		}

		static IVsHierarchyItem NavigateToItem (this IVsHierarchyItem item, string[] paths, int index)
		{
			if (index >= paths.Length || item == null)
				return item;

			var name = paths[index];

			var child = item.Children.FirstOrDefault (x => x.GetProperty<string> (VsHierarchyPropID.Name) == name) ??
				// Fall back to using the node text if not found by name.)
				item.Children.FirstOrDefault (x => x.Text == name);

			if (child == null)
				return null;

			return child.NavigateToItem (paths, ++index);
		}

		const string PropertyValue = "{0} = {1}";

		public static void Dump (this IVsHierarchyItem item, params int[] additionalProperties)
		{
			Dump (item, new DebugTestOutputHelper ());
		}

		public static void Dump (this IVsHierarchyItem item, ITestOutputHelper output, params int[] additionalProperties)
		{
			using (var indented = new IndentedTestOutputHeper (output)) {
				indented.WriteLine (PropertyValue, "Name", item.GetProperty<string> (VsHierarchyPropID.Name));
				indented.WriteLine (PropertyValue, "Text", item.Text);
				foreach (var propertyId in additionalProperties) {
					if (!WritePropertyIfDefined (item, indented, propertyId, typeof (__VSPROPID)))
						if (!WritePropertyIfDefined (item, indented, propertyId, typeof (__VSPROPID2)))
							if (!WritePropertyIfDefined (item, indented, propertyId, typeof (__VSPROPID3)))
								if (!WritePropertyIfDefined (item, indented, propertyId, typeof (__VSPROPID4)))
									if (!WritePropertyIfDefined (item, indented, propertyId, typeof (__VSPROPID5)))
										if (!WritePropertyIfDefined (item, indented, propertyId, typeof (VsHierarchyPropID)))
											indented.WriteLine (PropertyValue, propertyId, item.GetProperty (propertyId));
				}

				string fullPath;
				if (ErrorHandler.Succeeded(item.GetActualHierarchy ().GetCanonicalName (item.GetActualItemId (), out fullPath)) && File.Exists(fullPath))
					indented.WriteLine (PropertyValue, "FullPath", fullPath);

				if (item.Children.Any ()) {
					indented.WriteLine ("Children = ");
					using (var childIndent = new IndentedTestOutputHeper (indented, begin: "[", end: "]")) {
						foreach (var child in item.Children) {
							child.Dump (childIndent, additionalProperties);
						}
					}
				}
			}
		}

		static bool WritePropertyIfDefined (IVsHierarchyItem item, ITestOutputHelper output, int propertyId, Type enumType)
		{
			if (Enum.IsDefined (enumType, propertyId)) {
				output.WriteLine (PropertyValue,
					Enum.GetName (enumType, propertyId),
					item.GetProperty (propertyId));

				return true;
			}

			return false;
		}

		class DebugTestOutputHelper : ITestOutputHelper
		{
			ITestOutputHelper output;

			public DebugTestOutputHelper (ITestOutputHelper output = null)
			{
				this.output = output;
			}

			public void WriteLine (string message)
			{
				Debug.WriteLine (message);
				Debugger.Log (0, "", message + Environment.NewLine);
				if (output != null)
					output.WriteLine (message);
			}

			public void WriteLine (string format, params object[] args)
			{
				Debug.WriteLine (format, args);
				Debugger.Log (0, "", string.Format (format, args) + Environment.NewLine);
				if (output != null)
					output.WriteLine (format, args);
			}
		}

		class IndentedTestOutputHeper : ITestOutputHelper, IDisposable
		{
			ITestOutputHelper output;
			string end;

			public IndentedTestOutputHeper (ITestOutputHelper output, string begin = "{", string end = "}")
			{
				this.output = output;
				this.end = end;
				output.WriteLine (begin);
			}

			public void Dispose ()
			{
				output.WriteLine (end);
			}

			public void WriteLine (string message)
			{
				output.WriteLine ("  " + message);
			}

			public void WriteLine (string format, params object[] args)
			{
				output.WriteLine ("  " + format, args);
			}
		}
	}
}
