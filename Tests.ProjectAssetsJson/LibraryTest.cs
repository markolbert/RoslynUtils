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
    public class LibraryTest : ProjectAssetsTestBase
    {
        [Theory]
        [InlineData("library-single.json", "C:\\Programming\\RoslynUtils\\ProjectAssetsJson")]
        [InlineData("libraries.json", "C:\\Programming\\RoslynUtils\\ProjectAssetsJson")]
        public void Test(string jsonFile, string projDir )
        {
            var fileContents = ReadJsonFile( jsonFile );
            var numLibs = Regex.Matches( fileContents, "\"type\"" ).Count;
            var container = ConvertJsonSnippet(fileContents);

            var expando = GetProperty<ExpandoObject>(container, "libraries");
            expando.Should().NotBeNull();

            var libs = new List<ILibraryInfo>();

            foreach( var kvp in expando )
            {
                kvp.Value.Should().BeOfType<ExpandoObject>();
                var libContainer = (ExpandoObject) kvp.Value;

                var refType = GetEnum<ReferenceType>( libContainer, "type" );

                switch (refType)
                {
                    case ReferenceType.Package:
                        libs.Add(new PackageLibrary(kvp.Key, libContainer, LoggerFactory));
                        break;

                    case ReferenceType.Project:
                        libs.Add(new ProjectLibrary(kvp.Key, libContainer, projDir, LoggerFactory));
                        break;

                    default:
                        throw ProjectAssetsException.CreateAndLog(
                            $"Unsupported value '{refType}' for {typeof(ReferenceType)}",
                            this.GetType(),
                            Logger);
                }
            }

            libs.Count.Should().Be( numLibs );
        }
    }
}
