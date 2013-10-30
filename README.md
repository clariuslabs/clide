What is Clide?
=====

Clide is a managed, intuitive, modern and composable API for .NET-based IDEs extensibility and automation. Its goal is to provide a useful and comprehensive abstraction of common IDE automation APIs, so that extensions can remain agnostic of the specifics of a given development environment.

It leverages dependency injection, supports unit testing of automation and extensibility code, and provides useful primitives for both consuming services and tools as well as providing your own to the environment. 

The initial goal is to provide a unified API for Visual Studio and Xamarin Studio.

How do I get it?
=====

Install from [https://nuget.org/packages/Clide](https://nuget.org/packages/Clide "NuGet")

This currently installs the Visual Studio binding only. It will eventually migrate to Clide.VisualStudio and Clide.XamarinStudio, with the Clide package containing just the interfaces to make your automation code IDE-agnostic. Only the bootstrapping extension would need to be IDE-specific.

How can Clide help me?
=====

If you are authoring any kind of Visual Studio or Xamarin Studio tooling or automation, Clide can make things easier, by providing an intuitive API for the solution, tool windows, option pages, etc., while allowing your extensibility code to remain IDE-agnostic and reusable across both Visual Studio and Xamarin Studio.

Is there API documentation?
=====

Documentation will be published automatically via [NuDoq](http://www.nudoq.org/#!/Projects/Clide). It's still not live though.
