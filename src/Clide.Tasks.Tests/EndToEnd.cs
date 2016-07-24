using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using Microsoft.Build.Construction;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Execution;
using Microsoft.Build.Framework;
using Xunit;
using Xunit.Abstractions;
using System.Linq;
using Microsoft.CodeAnalysis.MSBuild;
using System.Diagnostics;

namespace Clide.Tasks.Tests
{
	public class EndToEnd : IDisposable
	{
		ITestOutputHelper output;

		public EndToEnd(ITestOutputHelper output)
		{
			this.output = output;
		}

		[Fact]
		public void when_building_clide_then_succeeds()
		{
			var projectFile = Path.Combine(ModuleInitializer.BaseDirectory, @"..\..\..\Clide\Clide.csproj");
			var properties = new Dictionary<string, string>
			{
				{ "CodeTaskAssembly", @"C:\Windows\Microsoft.NET\Framework\v4.0.30319\Microsoft.Build.Tasks.v4.0.dll" }
			};
			var request = new BuildRequestData(projectFile, properties, null, new[] { "Build" }, null);
			var parameters = new BuildParameters
			{
				GlobalProperties = properties,
				Loggers = new[] { new TestOutputLogger(output) }
			};

			var result = BuildManager.DefaultBuildManager.Build(parameters, request);

			Assert.True(result.HasResultsForTarget("Build"));
			Assert.Equal(TargetResultCode.Success, result.ResultsByTarget["Build"].ResultCode);
		}

		[Fact]
		public void when_generating_exports_for_empty_project_then_succeeds_with_empty_result()
		{
			var result = BuildProject(new TestOutputLogger(output), (project, properties) => "Clide::GenerateExports");

			Assert.True(result.HasResultsForTarget("Clide::GenerateExports"));
			Assert.Equal(TargetResultCode.Success, result.ResultsByTarget["Clide::GenerateExports"].ResultCode);
			Assert.Equal(0, result.ResultsByTarget["Clide::GenerateExports"].Items.Length);
		}

		[Fact]
		public void when_finding_components_for_empty_project_then_succeeds_with_empty_result()
		{
			var result = BuildProject(new TestOutputLogger(output), (project, properties) => "Clide::FindComponents");

			Assert.True(result.HasResultsForTarget("Clide::FindComponents"));
			Assert.Equal(TargetResultCode.Success, result.ResultsByTarget["Clide::FindComponents"].ResultCode);
			Assert.Equal(0, result.ResultsByTarget["Clide::FindComponents"].Items.Length);
		}

		[Fact]
		public void when_finding_components_then_succeeds_and_finds_one()
		{
			var targetDir = Path.Combine(ModuleInitializer.BaseDirectory, MethodBase.GetCurrentMethod().Name);
			var result = BuildProject(new TestOutputLogger(output), (project, properties) =>
			{
				var componentFile = Path.Combine(targetDir, "Component.cs");
				if (!Directory.Exists(targetDir))
					Directory.CreateDirectory(targetDir);

				File.WriteAllText(componentFile, @"
using System;
using System.Reactive.Disposables;
using System.ComponentModel.Composition;

namespace Clide 
{
	[Component(CreationPolicy.Shared)]
	public partial class Producer : IObservable<DerivedEvent>
	{
		public IDisposable Subscribe(IObserver<DerivedEvent> observer)
		{
			observer.OnNext(new DerivedEvent());
			return Disposable.Empty;
		}
	}

	public class BaseEvent { }
	public class DerivedEvent : BaseEvent { }
}
");

				project.AddItem("Compile", "Component.cs");

				return "Clide::FindComponents";
			});

			Assert.True(result.HasResultsForTarget("Clide::FindComponents"));
			Assert.Equal(TargetResultCode.Success, result.ResultsByTarget["Clide::FindComponents"].ResultCode);
			Assert.Equal(1, result.ResultsByTarget["Clide::FindComponents"].Items.Length);
		}

		[Fact]
		public void when_generating_exports_then_succeeds_and_generates_one()
		{
			var targetDir = Path.Combine(ModuleInitializer.BaseDirectory, MethodBase.GetCurrentMethod().Name);
			var result = BuildProject(new TestOutputLogger(output), (project, properties) =>
			{
				var componentFile = Path.Combine(targetDir, "Component.cs");
				if (!Directory.Exists(targetDir))
					Directory.CreateDirectory(targetDir);

				File.WriteAllText(componentFile, @"
using System;
using System.Reactive.Disposables;
using System.ComponentModel.Composition;

namespace Clide 
{
	[Component(CreationPolicy.Shared)]
	public partial class Producer : IObservable<DerivedEvent>
	{
		public IDisposable Subscribe(IObserver<DerivedEvent> observer)
		{
			observer.OnNext(new DerivedEvent());
			return Disposable.Empty;
		}
	}

	public class BaseEvent { }
	public class DerivedEvent : BaseEvent { }
}
");

				project.AddItem("Compile", "Component.cs");

				return "Clide::ExportComponents";
			});

			Assert.True(result.HasResultsForTarget("Clide::ExportComponents"));
			Assert.Equal(TargetResultCode.Success, result.ResultsByTarget["Clide::ExportComponents"].ResultCode);
			Assert.Equal(1, result.ResultsByTarget["Clide::ExportComponents"].Items.Length);
		}

		[Fact]
		public void when_compiling_twice_then_does_not_cause_new_compilation()
		{
			var logger = new TestOutputLogger(output);
			var targetDir = Path.Combine(ModuleInitializer.BaseDirectory, MethodBase.GetCurrentMethod().Name);
			var targetFileName = Path.Combine(targetDir, "bin", "Debug", 
				Path.GetFileName(Assembly.GetExecutingAssembly().ManifestModule.FullyQualifiedName));
			var componentFile = Path.Combine(targetDir, "Component.cs");

			var firstBuild = BuildProject(logger, (project, properties) =>
			{
				if (!Directory.Exists(targetDir))
					Directory.CreateDirectory(targetDir);

				File.WriteAllText(componentFile, @"
using System;
using System.Reactive.Disposables;
using System.ComponentModel.Composition;

namespace Clide 
{
	[Component(CreationPolicy.Shared)]
	public partial class Producer : IObservable<DerivedEvent>
	{
		public IDisposable Subscribe(IObserver<DerivedEvent> observer)
		{
			observer.OnNext(new DerivedEvent());
			return Disposable.Empty;
		}
	}

	public class BaseEvent { }
	public class DerivedEvent : BaseEvent { }
}
");

				project.AddItem("Compile", "Component.cs");

				return "Build";
			});

			Assert.True(firstBuild.HasResultsForTarget("Build"));
			Assert.Equal(TargetResultCode.Success, firstBuild.ResultsByTarget["Build"].ResultCode);
			var firstPassTasks = logger.FinishedTasks.ToArray();

			var firstTimestamp = File.GetLastWriteTime(targetFileName);
			Assert.True(firstPassTasks.Any(e => e.TaskName == "FindComponents"));
			Assert.True(firstPassTasks.Any(e => e.TaskName == "ExportComponents"));

			var projectFile = Path.Combine(targetDir, "Test.csproj");
			var request = new BuildRequestData(projectFile, new Dictionary<string, string>(), null, new[] { "Build" }, null);
			var parameters = new BuildParameters
			{
				GlobalProperties = new Dictionary<string, string>(),
					UseSynchronousLogging = true,
				Loggers = new[] { logger }
			};

			logger.Reset();
			var secondBuild = BuildManager.DefaultBuildManager.Build(parameters, request);
			var secondPassTasks = logger.FinishedTasks.ToArray();
			var secondTimestamp = File.GetLastWriteTime(targetFileName);

			Assert.False(secondPassTasks.Any(e => e.TaskName == "FindComponents"), "FindComponents shouldn't have been executed on the second build.");
			Assert.False(secondPassTasks.Any(e => e.TaskName == "ExportComponents"), "ExportComponents shouldn't have been executed on the second build.");
			Assert.Equal(firstTimestamp, secondTimestamp);

			File.SetLastWriteTime(componentFile, DateTime.Now);

			logger.Reset();
			var thirdBuild = BuildManager.DefaultBuildManager.Build(parameters, request);
			var thirdPassTasks = logger.FinishedTasks.ToArray();
			var thirdTimestamp = File.GetLastWriteTime(targetFileName);

			Assert.True(thirdPassTasks.Any(e => e.TaskName == "FindComponents"));
			Assert.True(thirdPassTasks.Any(e => e.TaskName == "ExportComponents"));
			Assert.NotEqual(secondTimestamp, thirdTimestamp);
		}

		BuildResult BuildProject(ILogger logger, Func<ProjectRootElement, Dictionary<string, string>, string> configure, [CallerMemberName] string caller = null)
		{
			var project = ProjectRootElement.Create(new ProjectCollection());
			project.AddItem("Reference", "mscorlib");
			var template = new Project(Path.Combine(ModuleInitializer.BaseDirectory, @"..\..\Clide.Tasks.Tests.csproj"),
				new Dictionary<string, string>
				{
					{ "CodeTaskAssembly", @"C:\Windows\Microsoft.NET\Framework\v4.0.30319\Microsoft.Build.Tasks.v4.0.dll" }
				}, null, new ProjectCollection());
			var items = template.GetItems("Reference");

			foreach (var item in items)
			{
				if (item.HasMetadata("HintPath"))
					project.AddItem(item.ItemType, item.UnevaluatedInclude, new[]
					{
						new KeyValuePair<string, string>("HintPath", new FileInfo(
							Path.Combine(ModuleInitializer.BaseDirectory, @"..\..", item.GetMetadata("HintPath").UnevaluatedValue)).FullName)
					});
				else
					project.AddItem(item.ItemType, item.UnevaluatedInclude);
			}

			foreach (var prop in template.Properties)
			{
				if (!prop.IsEnvironmentProperty &&
					!prop.IsGlobalProperty &&
					!prop.IsImported &&
					!prop.IsReservedProperty)
					project.AddProperty(prop.Name, prop.UnevaluatedValue);
			}

			project.AddProperty("GenerateExports", "true");

			foreach (var item in Directory.EnumerateFiles(Path.Combine(ModuleInitializer.BaseDirectory, "Clide"), "*.cs"))
			{
				project.AddItem("Compile", $"{ModuleInitializer.BaseDirectory}\\Clide\\{Path.GetFileName(item)}");
			}

			var targetDir = Path.Combine(ModuleInitializer.BaseDirectory, caller);
			if (Directory.Exists(targetDir))
				Directory.Delete(targetDir, true);

			Directory.CreateDirectory(targetDir);

			var properties = new Dictionary<string, string>
			{
				//{ "MSBuildProjectFullPath", projectFile }
			};

			var target = configure(project, properties);

			project.AddImport(@"$(MSBuildToolsPath)\Microsoft.CSharp.targets");
			project.AddImport(Path.Combine(ModuleInitializer.BaseDirectory, @"..\..\..\Clide.Tasks\bin\Clide.targets"));

			var projectFile = Path.Combine(targetDir, "Test.csproj");
			if (!Directory.Exists(targetDir))
				Directory.CreateDirectory(targetDir);

			Debug.WriteLine($"Created test project {projectFile}");

			project.Save(projectFile);

			var request = new BuildRequestData(projectFile, properties, null, new[] { target }, null);
			var parameters = new BuildParameters
			{
				GlobalProperties = properties,
				Loggers = new[] { logger }
			};

			return BuildManager.DefaultBuildManager.Build(parameters, request);
		}

		public void Dispose()
		{
			BuildManager.DefaultBuildManager.Dispose();
		}
	}
}
