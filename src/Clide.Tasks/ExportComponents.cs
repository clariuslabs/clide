using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Microsoft.CodeAnalysis.Text;

namespace Clide.Tasks
{
	public class ExportComponents : CSharpTask
	{
		[Required]
		public string IntermediateOutputPath { get; set; }

		[Required]
		public ITaskItem[] ComponentFiles { get; set; }

		public ITaskItem[] ExcludeInterfaceNamespaces { get; set; }

		[Output]
		public ITaskItem[] OutputFiles { get; set; }

		public override bool Execute()
		{
			return base.Execute() && DoExecute();
		}

		bool DoExecute()
		{
			var documents = Project.FindDocuments(ComponentFiles, Cancellation);
			if (Cancellation.IsCancellationRequested)
				return false;

			var generator = new ExportsGenerator(Compilation);
			var exports = generator.GenerateExports(documents.Select(doc => doc.Value), 
				ExcludeInterfaceNamespaces == null ? 
					new HashSet<string>() : 
					new HashSet<string>(ExcludeInterfaceNamespaces.Select(x => x.ItemSpec)),
				Cancellation);

			if (Cancellation.IsCancellationRequested)
				return false;

			var outputs = new List<ITaskItem>();
			foreach (var export in exports)
			{
				if (Cancellation.IsCancellationRequested)
					return false;

				var targetDir = Path.Combine(IntermediateOutputPath, Path.Combine(export.Folders.ToArray()));
				var targetFile = Path.Combine(targetDir, export.Name);
				if (!Directory.Exists(targetDir))
					Directory.CreateDirectory(targetDir);

				using (var writer = new StreamWriter(targetFile, false))
				{
					var text = export.GetTextAsync(Cancellation).Result;
					if (Cancellation.IsCancellationRequested)
						return false;

					text.Write(writer);
				}

				outputs.Add(new TaskItem(targetFile));
			}

			OutputFiles = outputs.ToArray();

			return true;
		}
	}
}
