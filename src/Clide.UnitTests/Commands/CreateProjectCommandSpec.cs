using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Linq;
using Xunit;

namespace Clide
{
    public class CreateProjectCommandSpec
    {
        CreateProjectHandler handler;
        string lastCommand;

        public CreateProjectCommandSpec()
        {
            handler = new CreateProjectHandler(x => lastCommand = x);
        }

        [Fact]
        public void when_required_inputs_are_null_then_command_is_disabled()
        {
            // Template and Output are required
            Assert.False(handler.CanExecute(new CreateProjectCommand { Template = null }));
            Assert.False(handler.CanExecute(new CreateProjectCommand { Template = "foo", Output = null }));
        }

        [Fact]
        public void when_creating_project_then_dotnet_new_command_is_executed()
        {
            var output = GenerateTempOutput();
            var projects = handler.Execute(
                new CreateProjectCommand
                {
                    Template = "classlib",
                    Output = output
                }).ToList();

            Assert.Contains("dotnet new classlib", lastCommand);
            Assert.Contains($"-o \"{output}", lastCommand);
        }

        string GenerateTempOutput() =>
            Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
    }
}
