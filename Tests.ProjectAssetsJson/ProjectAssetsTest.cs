using System.Dynamic;
using FluentAssertions;
using J4JSoftware.Roslyn;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Tests.ProjectAssetsJson
{
    public class ProjectAssetsTest : ProjectAssetsTestBase
    {
        [Theory]
        [InlineData(
            "C:\\Programming\\RoslynUtils\\ProjectAssetsJson\\obj\\project.assets.json", 
            "C:\\Programming\\RoslynUtils\\ProjectAssetsJson\\ProjectAssetsJson.csproj"
            )]
        public void Test(string jsonFile, string projFile )
        {
            var projAssets = new ProjectAssets(
                ServiceProvider.Instance.GetRequiredService<JsonProjectAssetsConverter>(), LoggerFactory );

            projAssets.Initialize( projFile, jsonFile ).Should().BeTrue();
        }
    }
}