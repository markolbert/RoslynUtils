using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using FluentAssertions;
using J4JSoftware.Roslyn;
using Microsoft.CodeAnalysis.Text;
using Microsoft.Extensions.DependencyInjection;
using NuGet.Versioning;
using Xunit;

namespace Tests.ProjectAssetsJson
{
    public class RoslynWalkerTestBase
    {
        [ Theory ]
        [ InlineData( "C:\\Programming\\J4JLogging\\J4JLogging\\J4JLogging.csproj", "netstandard2.1" ) ]
        public void Test( string projFilePath, string tgtFWText )
        {
            TargetFramework.Create( tgtFWText, TargetFrameworkTextStyle.Simple, out var tgtFW ).Should().BeTrue();

            var projModel = ServiceProvider.Instance.GetRequiredService<ProjectModel>();

            var result = projModel.Compile( projFilePath );

            if( !result )
            {
                foreach( var diagnostic in projModel.Diagnostics )
                {
                    if( diagnostic.Location.SourceTree == null )
                    {
                        System.Diagnostics.Debug.WriteLine(
                            $"[{diagnostic.Severity}({diagnostic.Id})] No source information available" );
                        continue;
                    }

                    var errorLines = diagnostic.Location.GetLineSpan();
                    var sourceText = diagnostic.Location.SourceTree!.GetText();

                    for( var lineNum = errorLines.StartLinePosition.Line; lineNum <= errorLines.EndLinePosition.Line; lineNum++ )
                    {
                        System.Diagnostics.Debug.WriteLine(
                            $"[{diagnostic.Severity}({diagnostic.Id})] {diagnostic.GetMessage()} {sourceText.Lines[ lineNum ]}" );
                    }
                }
            }

            result.Should().BeTrue();
        }
    }
}
