using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using FluentAssertions;
using Xunit.Sdk;

namespace Tests.DocCompiler
{
    public static class TokenizerMemberData
    {
        public static IEnumerable<object[]> GetTokenizerData()
        {
            var retVal = new List<object[]>();

            foreach( var entry in GetData( Path.Combine( Environment.CurrentDirectory, "tokenizer-data", "tokenizer-data.json" ) ) )
            {
                var items = new object[3];
                retVal.Add( items );

                items[ 0 ] = entry.SourceCode;
                items[ 1 ] = entry.Success;
                items[ 2 ] = entry.Tokens;
            }

            return retVal;
        }

        private static List<TokenizerData> GetData( string fileName )
        {
            File.Exists( fileName ).Should().BeTrue();

            var options = new JsonSerializerOptions
            {
                Converters = { new JsonStringEnumConverter() }
            };

            var retVal = JsonSerializer.Deserialize<List<TokenizerData>>( File.ReadAllText( fileName ), options );
            retVal.Should().NotBeNull();

            return retVal!;
        }
    }
}
