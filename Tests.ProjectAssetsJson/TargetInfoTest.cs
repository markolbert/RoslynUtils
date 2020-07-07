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
    public class TargetInfoTest : TestBase
    {
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
                
                var tgtInfo = new TargetInfo( kvp.Key, (ExpandoObject) kvp.Value, LoggerFactory );
                targets.Add( tgtInfo );
            }

            targets.Count.Should().Be( 1 );
            targets[ 0 ].Packages.Count.Should().Be( numPackages );
        }
    }
}
