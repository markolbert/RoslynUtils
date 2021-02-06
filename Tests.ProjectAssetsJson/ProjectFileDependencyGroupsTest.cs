using System.Collections.Generic;
using System.Dynamic;
using FluentAssertions;
using J4JSoftware.Roslyn;
using J4JSoftware.Roslyn.ProjectAssets;
using Xunit;

namespace Tests.ProjectAssetsJson
{
    public class ProjectFileDependencyGroupsTest : ProjectAssetsTestBase
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
                var libContainer = (List<string>) kvp.Value!;

                pfdg.Add( new ProjectFileDependencyGroup( kvp.Key, libContainer, LoggerFactory ) );
            }

            pfdg.Count.Should().Be( numGroups );
        }
    }
}
