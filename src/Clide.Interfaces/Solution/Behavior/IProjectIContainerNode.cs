namespace Clide
{
    /// <summary>
    /// Represents a solution-level node that contains projects.
    /// </summary>
    public interface IProjectContainerNode
    {
        IProjectNode UnfoldTemplate(string templateId, string projectName, string language = "CSharp");
    }
}