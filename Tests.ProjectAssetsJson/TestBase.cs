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
    public class TestBase : ProjectAssetsBase
    {
        private readonly JsonSerializerOptions _converterOptions;

        protected TestBase()
            : base( ServiceProvider.Instance.GetRequiredService<Func<IJ4JLogger>>() )
        {
            _converterOptions = new JsonSerializerOptions();
            _converterOptions.Converters.Add(
                ServiceProvider.Instance.GetRequiredService<JsonProjectAssetsConverter>() );
        }

        protected string GetFullPath( string path ) =>
            Path.Combine( Environment.CurrentDirectory, "json-snippets", path );

        protected string ReadJsonFile( string jsonFile )
        {
            var filePath = GetFullPath( jsonFile );

            return File.Exists( filePath ) ? File.ReadAllText( filePath ) : string.Empty;
        }

        protected ExpandoObject ConvertJsonSnippet( string jsonContent ) =>
            string.IsNullOrEmpty( jsonContent )
                ? new ExpandoObject()
                : JsonSerializer.Deserialize<ExpandoObject>( jsonContent, _converterOptions );
    }
}
