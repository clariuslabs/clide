using Clide.Sdk.Solution;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Clide.Solution
{
	[DisplayName(
@"Feature: MSBuild Properties Access
	As a developer
	I want to be able to access MSBuild properties on solution elements easily
	So that I don't have to resort to convoluted APIs
")]
	public partial class Dynamic_Properties_Feature
	{
		[TestClass]
		public abstract class Given_A_Solution_With_A_Library_Project : VsHostedSpec
		{
			static readonly IAssertion Assert = new Assertion();
			const string MsBuildXmlNs = "{http://schemas.microsoft.com/developer/msbuild/2003}";

			Func<dynamic, string> getExistingPropertyDynamic;
			Action<dynamic, string> setExistingPropertyDynamic;
			string existingPropertyName;
			string existingPropertyValue;

			/// <summary>
			/// Initializes a new instance of the <see cref="Given_A_Solution_With_A_Library_Project"/> class.
			/// </summary>
			/// <param name="getExistingPropertyDynamic">
			/// A getter function for an existing property, to be used on 
			/// test that require dynamic syntax access (so the property has to 
			/// be known at compile time, therefore, we put that in a delegate).
			/// </summary>
			/// </param>
			/// <param name="setExistingPropertyDynamic">
			/// A setter function for an existing property, to be used on tests that 
			/// require dynamic syntax access.
			/// </param>
			/// <param name="existingPropertyName">
			/// The name of an existing property in the <see cref="Properties"/> bag.
			/// </param>
			public Given_A_Solution_With_A_Library_Project(Func<dynamic, string> getExistingPropertyDynamic, Action<dynamic, string> setExistingPropertyDynamic, 
				string existingPropertyName, string existingPropertyValue)
			{
				this.getExistingPropertyDynamic = getExistingPropertyDynamic;
				this.setExistingPropertyDynamic = setExistingPropertyDynamic;
				this.existingPropertyName = existingPropertyName;
				this.existingPropertyValue = existingPropertyValue;
			}

			[TestInitialize]
			public override void TestInitialize()
			{
				base.TestInitialize();

				base.OpenSolution("SampleSolution\\SampleSolution.sln");

				Solution = DevEnv.Get(ServiceProvider).SolutionExplorer().Solution;
				Library = Solution.FindProjects(p => p.DisplayName == "ClassLibrary").First();
			}

			public ISolutionNode Solution { get; private set; }
			public IProjectNode Library { get; private set; }

			/// <summary>
			/// The dynamic properties object under test.
			/// </summary>
			protected abstract dynamic Properties { get; }

			protected virtual string ProjectFile { get { return Library.PhysicalPath; } }

			[HostType("VS IDE")]
			[TestMethod]
			public virtual void when_I_get_an_existing_property_then_I_can_use_indexer_property_syntax()
			{
				string value = Properties[existingPropertyName];

				Assert.Equal(existingPropertyValue, value);
			}

			[HostType("VS IDE")]
			[TestMethod]
			public virtual void when_I_get_a_non_existent_property_then_returns_null_with_dynamic_property_syntax()
			{
				string name = Properties.Foo;

				Assert.Null(name);
			}

			[HostType("VS IDE")]
			[TestMethod]
			public virtual void when_I_get_a_non_existent_property_then_returns_null_with_indexer_property_syntax()
			{
				string name = Properties["Foo"];

				Assert.Null(name);
			}

			[HostType("VS IDE")]
			[TestMethod]
			public virtual void when_I_get_existing_property_then_I_can_use_dynamic_property_syntax()
			{
				string name = getExistingPropertyDynamic(Properties);

				Assert.Equal(existingPropertyValue, name);
			}

			[HostType("VS IDE")]
			[TestMethod]
			public virtual void when_I_set_existing_property_then_I_can_use_dynamic_property_syntax()
			{
				string original = getExistingPropertyDynamic(Properties);

				setExistingPropertyDynamic(Properties, "Foo");

				string changed = getExistingPropertyDynamic(Properties);

				Assert.NotEqual(original, changed, "Existing property value was not changed by {0} on {1}", this.GetType().Name, ProjectFile);

				Library.Save();

				var saved = XDocument.Load(ProjectFile)
					.Root
					.Descendants(MsBuildXmlNs + existingPropertyName)
					.Select(e => e.Value)
					.First();

				Assert.Equal("Foo", saved);
			}

			[HostType("VS IDE")]
			[TestMethod]
			public virtual void when_I_set_existing_property_then_I_can_use_indexer_property_syntax()
			{
				string original = Properties[existingPropertyName];

				Properties[existingPropertyName] = "Foo";

				Assert.NotEqual(original, (string)Properties[existingPropertyName], "Existing property value was not changed by {0} on {1}", this.GetType().Name, ProjectFile);

				Library.Save();

				var saved = XDocument.Load(ProjectFile)
					.Root
					.Descendants(MsBuildXmlNs + existingPropertyName)
					.Select(e => e.Value)
					.First();

				Assert.Equal("Foo", saved);
			}
		}

		[TestClass]
		[DisplayName(
@"Scenario: Accessing Global MSBuild Project Properties
	Given an opened solution
	And a class library project
")]
		public class Accessing_Global_Project_Properties_Scenario : Given_A_Solution_With_A_Library_Project
		{
			public Accessing_Global_Project_Properties_Scenario()
				: base(GetProperty, SetProperty, "GlobalProperty", "GlobalValue")
			{
			}

			protected override dynamic Properties
			{
				get { return Library.Properties; }
			}

			private static string GetProperty(dynamic properties)
			{
				return properties.GlobalProperty;
			}

			private static void SetProperty(dynamic properties, string value)
			{
				properties.GlobalProperty = value;
			}
		}

		[TestClass]
		[DisplayName(
@"Scenario: Accessing Configuration-specific MSBuild Project Properties
	Given an opened solution
	And a class library project
")]
		public class Accessing_Configuration_Project_Properties_Scenario : Given_A_Solution_With_A_Library_Project
		{
			public Accessing_Configuration_Project_Properties_Scenario()
				: base(GetProperty, SetProperty, "ConfigProperty", "ConfigValue")
			{
			}

			protected override dynamic Properties
			{
				get { return Library.PropertiesFor("Debug|AnyCPU"); }
			}

			private static string GetProperty(dynamic properties)
			{
				return properties.ConfigProperty;
			}

			private static void SetProperty(dynamic properties, string value)
			{
				properties.ConfigProperty = value;
			}
		}

		[TestClass]
		[DisplayName(
@"Scenario: Accessing per-user MSBuild Project Properties
	Given an opened solution
	And a class library project
")]
		public class Accessing_User_Project_Properties_Scenario : Given_A_Solution_With_A_Library_Project
		{
			public Accessing_User_Project_Properties_Scenario()
				: base(GetProperty, SetProperty, "UserProperty", "UserValue")
			{
			}

			protected override dynamic Properties
			{
				get { return Library.UserProperties; }
			}

			protected override string ProjectFile { get { return base.ProjectFile + ".user"; } }

			private static string GetProperty(dynamic properties)
			{
				return properties.UserProperty;
			}

			private static void SetProperty(dynamic properties, string value)
			{
				properties.UserProperty = value;
			}
		}

		[TestClass]
		[DisplayName(
@"Scenario: Accessing per-user, per-config MSBuild Project Properties
	Given an opened solution
	And a class library project
")]
		public class Accessing_User_And_Config_Project_Properties_Scenario : Given_A_Solution_With_A_Library_Project
		{
			public Accessing_User_And_Config_Project_Properties_Scenario()
				: base(GetProperty, SetProperty, "UserConfigProperty", "UserConfigValue")
			{
			}

			protected override dynamic Properties
			{
				get { return Library.UserPropertiesFor("Debug|AnyCPU"); }
			}

			protected override string ProjectFile { get { return base.ProjectFile + ".user"; } }

			private static string GetProperty(dynamic properties)
			{
				return properties.UserConfigProperty;
			}

			private static void SetProperty(dynamic properties, string value)
			{
				properties.UserConfigProperty = value;
			}
		}
	}
}
