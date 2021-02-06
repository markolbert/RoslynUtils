using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text.RegularExpressions;
using FluentAssertions;
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

            var asDict = (IDictionary<string, object>) expando!;
            var folders = asDict.Keys.ToList();

            folders.Count.Should().Be( numFolders );
        }
    }
}
