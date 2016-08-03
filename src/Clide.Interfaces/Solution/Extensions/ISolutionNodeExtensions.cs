namespace Clide
{
	public static partial class SolutionExtensions
	{
		public static IProjectNode UnfoldTemplate(this ISolutionNode solution, string templateId, string projectName, string language = "CSharp") =>
			solution.AsProjectContainerNode().UnfoldTemplate(templateId, projectName, language);
	}
}