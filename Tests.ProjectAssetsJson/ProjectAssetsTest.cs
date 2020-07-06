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
    public class ProjectAssetsTest : ConfigurationBase
    {
        private readonly JsonSerializerOptions _converterOptions;
        private readonly Func<IJ4JLogger> _loggerFactory;

        public ProjectAssetsTest()
        {
            _converterOptions = new JsonSerializerOptions();
            
            var converter = ServiceProvider.Instance.GetRequiredService<JsonProjectAssetsConverter>();
            _converterOptions.Converters.Add(converter);

            _loggerFactory = ServiceProvider.Instance.GetRequiredService<Func<IJ4JLogger>>();
        }

        private string ReadJsonFile( string jsonFile )
        {
            var filePath = Path.Combine(Environment.CurrentDirectory, "json-snippets", jsonFile);

            return File.Exists( filePath ) ? File.ReadAllText( filePath ) : string.Empty;
        }

        private ExpandoObject ConvertJsonSnippet( string jsonContent ) =>
            string.IsNullOrEmpty( jsonContent )
                ? new ExpandoObject()
                : JsonSerializer.Deserialize<ExpandoObject>( jsonContent, _converterOptions );

        private TProp GetProperty<TProp>(
            ExpandoObject container,
            string propName,
            bool caseSensitive = false,
            bool optional = false)
        {
            if (string.IsNullOrEmpty(propName))
                return default!;

            var asDict = (IDictionary<string, object>)container;

            // ExpandoObject keys are always case sensitive...so if we want a case insensitive match we have to 
            // go a bit convoluted...
            bool hasKey = false;

            if (caseSensitive) hasKey = asDict.ContainsKey(propName);
            else
            {
                // case insensitive matches
                switch (asDict.Keys.Count(k => k.Equals(propName, StringComparison.OrdinalIgnoreCase)))
                {
                    case 0:
                        // no match; key not found so default value of hasKey is okay
                        break;

                    case 1:
                        // replace the case-insensitive property name with the correctly-cased value
                        propName = asDict.Keys.First(k => k.Equals(propName, StringComparison.OrdinalIgnoreCase));
                        hasKey = true;

                        break;

                    default:
                        // multiple case-insensitive matches; case insensitive doesn't work
                        break;
                }
            }

            // it's okay if optional properties don't exist
            if (!hasKey && optional)
                return default!;

            if (asDict[propName] is TProp retVal)
                return retVal;

            LogAndThrow($"Could not find property", propName, typeof(ExpandoObject));

            // we'll never get here but need to keep the compiler happy...
            return default!;
        }

        [Theory]
        [InlineData("ProjectAssetsJson.txt", "project.assets.json")]
        public void ProjectAssets( string projFile, string jsonFile )
        {
            var projAssets = ServiceProvider.Instance.GetRequiredService<ProjectAssets>();

            projAssets.Initialize(Path.Combine(Environment.CurrentDirectory, "json-snippets", projFile),
                    Path.Combine( Environment.CurrentDirectory, "json-snippets", jsonFile ) )
                .Should().BeTrue();
        }

        [Theory]
        [InlineData("targets-one-package.json")]
        [InlineData("targets.json")]
        public void SingleTargetInfo(string jsonFile )
        {
            var fileContents = ReadJsonFile( jsonFile );
            var numPackages = Regex.Matches( fileContents, "type" ).Count;
            var container = ConvertJsonSnippet(fileContents);

            var expando = GetProperty<ExpandoObject>(container, "targets");
            expando.Should().NotBeNull();

            var targets = new List<TargetInfo>();

            foreach( var kvp in expando )
            {
                kvp.Value.Should().BeOfType<ExpandoObject>();
                
                var tgtInfo = new TargetInfo( kvp.Key, (ExpandoObject) kvp.Value, _loggerFactory );
                targets.Add( tgtInfo );
            }

            targets.Count.Should().Be( 1 );
            targets[ 0 ].Packages.Count.Should().Be( numPackages );
        }
    }
}
