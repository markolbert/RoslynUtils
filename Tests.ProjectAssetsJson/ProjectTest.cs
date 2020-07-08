using System.Dynamic;
using FluentAssertions;
using J4JSoftware.Roslyn;
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
