using System.Collections.Generic;
using System.Dynamic;
using System.Text.RegularExpressions;
using FluentAssertions;
using J4JSoftware.Roslyn;
using J4JSoftware.Roslyn.ProjectAssets;
using Xunit;

namespace Tests.ProjectAssetsJson
{
    public class TargetInfoTest : ProjectAssetsTestBase
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
                
                var tgtInfo = new TargetInfo( kvp.Key, (ExpandoObject) kvp.Value!, LoggerFactory );
                targets.Add( tgtInfo );
            }

            targets.Count.Should().Be( 1 );
            targets[ 0 ].Packages.Count.Should().Be( numPackages );
        }
    }
}
