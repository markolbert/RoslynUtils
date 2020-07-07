using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.RegularExpressions;
using FluentAssertions;
using J4JSoftware.Logging;
using J4JSoftware.Roslyn;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Serialization;
using Serilog.Core;
using Xunit;

namespace Tests.ProjectAssetsJson
{
    public class ProjectFileDependencyGroupsTest : TestBase
    {
        [Theory]
        [InlineData("project-file-dependency.json", 1)]
        public void Test(string jsonFile, int numGroups )
        {
            var fileContents = ReadJsonFile( jsonFile );
            var container = ConvertJsonSnippet(fileContents);

            var expando = GetProperty<ExpandoObject>(container, "projectFileDependencyGroups");
            expando.Should().NotBeNull();

            var pfdg = new List<ProjectFileDependencyGroup>();

            foreach( var kvp in expando )
            {
                kvp.Value.Should().BeOfType<List<string>>();
                var libContainer = (List<string>) kvp.Value;

                pfdg.Add( new ProjectFileDependencyGroup( kvp.Key, libContainer, LoggerFactory ) );
            }

            pfdg.Count.Should().Be( numGroups );
        }
    }
}
