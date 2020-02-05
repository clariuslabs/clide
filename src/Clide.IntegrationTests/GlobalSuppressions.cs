
// This file is used by Code Analysis to maintain SuppressMessage 
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given 
// a specific target and scoped to a namespace, type, member, etc.

[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "We use lower-case BDD style naming for tests")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "xUnit1024:Test methods cannot have overloads", Justification = "We re-declare the methods in the derived class to annotate with inline data/theory.")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "VSTHRD200:Use \"Async\" suffix for async methods", Justification = "For unit tests, no public API.")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "VSTHRD012:Provide JoinableTaskFactory where allowed", Justification = "For unit tests, no public API.")]
