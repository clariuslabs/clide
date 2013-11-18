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

namespace Clide.Solution
{

    /// <summary>
    /// Provides a hierarchical visitor pattern interface for the solution model.
    /// </summary>
    public interface ISolutionVisitor
    {
        /// <summary>
        /// Begins visiting the solution.
        /// </summary>
        /// <param name="solution">The solution being visited.</param>
        /// <returns><see langword="true"/> if the solution child nodes should be visited; <see langword="false"/> otherwise.</returns>
        bool VisitEnter(ISolutionNode solution);

        /// <summary>
        /// Ends visiting the solution.
        /// </summary>
        /// <param name="solution">The solution being visited.</param>
        /// <returns>The result of the solution traversal operation.</returns>
        bool VisitLeave(ISolutionNode solution);

        /// <summary>
        /// Begins visiting a solution item.
        /// </summary>
        /// <param name="solutionItem">The solution item being visited.</param>
        /// <returns><see langword="true"/> if the solution item children should be visited; <see langword="false"/> otherwise.</returns>
        bool VisitEnter(ISolutionItemNode solutionItem);

        /// <summary>
        /// Ends visiting a solution item.
        /// </summary>
        /// <param name="solutionItem">The solution item being visited.</param>
        /// <returns><see langword="true"/> if the solution item siblings should be visited; <see langword="false"/> otherwise.</returns>
        bool VisitLeave(ISolutionItemNode solutionItem);

        /// <summary>
        /// Begins visiting a solution folder.
        /// </summary>
        /// <param name="solutionFolder">The solution folder being visited.</param>
        /// <returns><see langword="true"/> if the solution folder children should be visited; <see langword="false"/> otherwise.</returns>
        bool VisitEnter(ISolutionFolderNode solutionFolder);

        /// <summary>
        /// Ends visiting a solution folder.
        /// </summary>
        /// <param name="solutionFolder">The solution folder being visited.</param>
        /// <returns><see langword="true"/> if the solution folder siblings should be visited; <see langword="false"/> otherwise.</returns>
        bool VisitLeave(ISolutionFolderNode solutionFolder);

        /// <summary>
        /// Begins visiting a project.
        /// </summary>
        /// <param name="project">The project being visited.</param>
        /// <returns><see langword="true"/> if the project children should be visited; <see langword="false"/> otherwise.</returns>
        bool VisitEnter(IProjectNode project);

        /// <summary>
        /// Ends visiting a project.
        /// </summary>
        /// <param name="project">The project being visited.</param>
        /// <returns><see langword="true"/> if the project siblings should be visited; <see langword="false"/> otherwise.</returns>
        bool VisitLeave(IProjectNode project);

        /// <summary>
        /// Begins visiting a project folder.
        /// </summary>
        /// <param name="folder">The project folder being visited.</param>
        /// <returns><see langword="true"/> if the project folder children should be visited; <see langword="false"/> otherwise.</returns>
        bool VisitEnter(IFolderNode folder);

        /// <summary>
        /// Ends visiting a project folder.
        /// </summary>
        /// <param name="folder">The project folder being visited.</param>
        /// <returns><see langword="true"/> if the project folder siblings should be visited; <see langword="false"/> otherwise.</returns>
        bool VisitLeave(IFolderNode folder);

        /// <summary>
        /// Begins visiting a project item.
        /// </summary>
        /// <param name="item">The project item being visited.</param>
        /// <returns><see langword="true"/> if the project item children should be visited; <see langword="false"/> otherwise.</returns>
        bool VisitEnter(IItemNode item);

        /// <summary>
        /// Ends visiting a project item.
        /// </summary>
        /// <param name="item">The project item being visited.</param>
        /// <returns><see langword="true"/> if the project item siblings should be visited; <see langword="false"/> otherwise.</returns>
        bool VisitLeave(IItemNode item);

        /// <summary>
        /// Begins visiting a project references.
        /// </summary>
        /// <param name="references">The project's references being visited.</param>
        /// <returns><see langword="true"/> if the project references should be visited; <see langword="false"/> otherwise.</returns>
        bool VisitEnter(IReferencesNode references);

        /// <summary>
        /// Begins visiting a project references.
        /// </summary>
        /// <param name="references">The project's references being visited.</param>
        /// <returns><see langword="true"/> if the project references siblings should be visited; <see langword="false"/> otherwise.</returns>
        bool VisitLeave(IReferencesNode references);

        /// <summary>
        /// Begins visiting a project reference.
        /// </summary>
        /// <param name="reference">The project reference being visited.</param>
        /// <returns><see langword="true"/> if the project reference children should be visited; <see langword="false"/> otherwise.</returns>
        bool VisitEnter(IReferenceNode reference);

        /// <summary>
        /// Ends visiting a project reference.
        /// </summary>
        /// <param name="reference">The project reference being visited.</param>
        /// <returns><see langword="true"/> if the project reference siblings should be visited; <see langword="false"/> otherwise.</returns>
        bool VisitLeave(IReferenceNode reference);
    }
}