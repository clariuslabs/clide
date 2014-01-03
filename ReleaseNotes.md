v2.2

* Added general-purpose referencing service

v2.1

* Added FindProjects extension method for ISolutionNode, which is more efficient than traversing the entire solution.
* Improved support for side-by-side versioning of Clide assemblies deployed by multiple extensions

v2.0

* Reworked internals to avoid depending on VS-MEF extensibility, which is problematic and may 
  cause interference with VS composition.
* Core DI now provided by Autofac, which gives us a performance boost in addition.
* Components now don't need any [Import] or [ImportingConstructor] in order for constructor
  dependencies to work, even for core VS services like IServiceProvider or IVsShell, DTE, etc.
* Types that have the ComponentAttribute applied are automatically registered. 
* Supports registering with a certain type (like [Export(typeof(IMyService))]): [Component(typeof(IMyService))]
* Added support for registering components as single instance (singletons) using [Component(IsSingleton = true)]

v1.4

* Fixes some performance issues due to MEF (lack of) caching
* Tracing of MEF composition off by default (can be turned on for diagnostics via the registry)
* Exposed ServiceLocator.GlobalProvider to get access to a thread-safe IServiceProvider for the entire Visual Studio IDE. 
* Removed usage of the problematic ServiceProvider.GlobalProvider (see above)
* Removed usage of the problematic ThreadHelper for UI thread marshaling (now leveraging the WPF Dispatcher)
* Added support for customizing what kinds of traces you want to receive on your output window via Host.Initialize
* Fixed a potential hang when using the Tracer from a background thread
* Fixed issue when a command filter was improperly configured.

v1.3

* Expose CompositionContainer directly on IDevEnv. This simplifies many scenarios where you want to interact with the container. Users can still import ICompositionService.
* Added support for intercepting arbitrary commands in the IDE via simple exports of ICommandInterceptor components, automatically registered when the host is initialized. 
* Moved MEF tracing/diagnostics to run on a separate background thread. This makes host initialization much faster.

v1.2

* Refactored for easier reuse by offering two static entry points: 
     - DevEnv.Get: gets the IDevEnv entry point API. This can be 
       used from anywhere you can import an IServiceProvider. By 
       default the instance is reused.
     - Host.Initialize: initializes a hosting package and its components
       (i.e. commands, filters, tool windows, etc.).
* Single assembly now can be used for both VS2010 and VS2012: this means 
  you no longer have to fork your extension for either IDE just because 
  you use Clide (you may because of other dependencies, like VSMSDK).

v1.1

* ISolutionExplorer API, with full solution traversal, behaviors
  for selecting, expanding and collapsing nodes, etc.
* Integration with Adapter pattern for pluggable conversion of 
  solution explorer nodes (i.e. projectNode.As&lt;EnvDTE.Project&gt;())
* Improved diagnostics and debug-time rendering of nodes
* Improved reporting of composition errors in the Clide output window

v1.0

* Full support for MEF-exported commands (ICommandExtension) and command filters
* ToolWindow support
* Tools options pages support
