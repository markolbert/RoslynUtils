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
    public class PackageFoldersTest : ProjectAssetsTestBase
    {
        [Theory]
        [InlineData("package-folders.json")]
        public void Test(string jsonFile )
        {
            var fileContents = ReadJsonFile( jsonFile );
            var container = ConvertJsonSnippet(fileContents);
            var numFolders = Regex.Matches( fileContents, "{" ).Count - 2;

            var expando = GetProperty<ExpandoObject>(container, "packageFolders");
            expando.Should().NotBeNull();

            var asDict = (IDictionary<string, object>) expando;
            var folders = asDict.Keys.ToList();

            folders.Count.Should().Be( numFolders );
        }
    }
}
