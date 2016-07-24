using System.Linq;
using Microsoft.Build.Framework;

namespace Clide.Tasks
{
	public class FindComponents : CSharpTask
	{
		[Required]
		public ITaskItem[] SourceFiles { get; set; }

		[Output]
		public ITaskItem[] ComponentFiles { get; set; }

		public override bool Execute()
		{
			return base.Execute() && DoExecute();
		}

		bool DoExecute()
		{
			// TODO: this isn't exactly cheap. We should cache the list 
			// of files we know are components, and make it a simple 
			// item list of files, dependent on @Compile.

			var documents = Project.FindDocuments(SourceFiles, Cancellation);
			if (Cancellation.IsCancellationRequested)
				return false;

			var attribute = Compilation.FindTypeByName("Clide", "Clide", "ComponentAttribute");
			if (attribute == null)
			{
				Log.LogWarning("Could not locate ComponentAttribute type from current compilation.");
				ComponentFiles = new ITaskItem[0];
			}
			else
			{
				ComponentFiles = documents
					.Where(doc => doc.Value.HasAttributedType(attribute, Cancellation))
					.Select(doc => doc.Key)
					.ToArray();
			}

			return true;
		}
	}
}