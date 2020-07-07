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
using NuGet.Versioning;
using Serilog.Core;
using Xunit;

namespace Tests.ProjectAssetsJson
{
    public class ProjectTest : TestBase
    {
        [Theory]
        [InlineData("project.json", 1, ProjectStyle.PackageReference)]
        public void Test(string jsonFile, int majorVersion, ProjectStyle style )
        {
            var fileContents = ReadJsonFile( jsonFile );
            var container = ConvertJsonSnippet(fileContents);

            var expando = GetProperty<ExpandoObject>(container, "project");
            expando.Should().NotBeNull();

            var project = new ProjectInfo( "project", expando, LoggerFactory );

            project.Version.Major.Should().Be( majorVersion );
            project.Restore.Should().NotBeNull();
            project.Restore.ProjectStyle.Should().Be( style );
        }
    }
}
