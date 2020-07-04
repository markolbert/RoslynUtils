using System;
using System.Dynamic;
using System.IO;
using System.Text.Json;
using FluentAssertions;
using J4JSoftware.Roslyn;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Serialization;
using Xunit;

namespace Tests.ProjectAssetsJson
{
    public class ProjectAssetsTest
    {
        private readonly JsonSerializerOptions _converterOptions;
        private readonly Func<TargetInfo> _tgtInfoCreator;

        public ProjectAssetsTest()
        {
            _converterOptions = new JsonSerializerOptions();
            
            var converter = ServiceProvider.Instance.GetRequiredService<JsonProjectAssetsConverter>();
            _converterOptions.Converters.Add(converter);

            _tgtInfoCreator = ServiceProvider.Instance.GetRequiredService<Func<TargetInfo>>();
        }

        private ProjectAssetsContext ConvertJsonSnippet( string jsonFile )
        {
            var filePath = Path.Combine( Environment.CurrentDirectory, "json-snippets", jsonFile );

            return new ProjectAssetsContext
            {
                RootContainer = File.Exists( filePath )
                    ? JsonSerializer.Deserialize<ExpandoObject>( File.ReadAllText( filePath ), _converterOptions )
                    : new ExpandoObject()
            };
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
        public void TargetInfo(string jsonFile)
        {
            var context = ConvertJsonSnippet(jsonFile);

            context.RootContainer.GetProperty<ExpandoObject>("targets", out var tgtDict)
                .Should().BeTrue();

            tgtDict.LoadFromContainer<TargetInfo, ExpandoObject>(_tgtInfoCreator, context, out var result)
                .Should().BeTrue();
        }
    }
}
