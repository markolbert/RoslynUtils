using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using FluentAssertions;
using J4JSoftware.Logging;
using J4JSoftware.Roslyn;
using Microsoft.CodeAnalysis.Text;
using Microsoft.Extensions.DependencyInjection;
using NuGet.Versioning;
using Xunit;

namespace Tests.ProjectAssetsJson
{
    public class RoslynWalkerTestBase
    {
        private readonly JsonProjectAssetsConverter _jsonConverter;
        private readonly Func<IJ4JLogger> _loggerFactory;

        public RoslynWalkerTestBase()
        {
            _jsonConverter = ServiceProvider.Instance.GetRequiredService<JsonProjectAssetsConverter>();
            _loggerFactory = ServiceProvider.Instance.GetRequiredService<Func<IJ4JLogger>>();
        }

        [ Theory ]
        [ InlineData( "C:\\Programming\\J4JLogging\\J4JLogging\\J4JLogging.csproj", "netstandard2.1" ) ]
        [ InlineData( "C:\\Programming\\J4JLogging\\ConsoleChannel\\ConsoleChannel.csproj", "netstandard2.1" ) ]
        public void CompilationTest( string projFilePath, string tgtFWText )
        {
            TargetFramework.Create( tgtFWText, TargetFrameworkTextStyle.Simple, out var tgtFW ).Should().BeTrue();

            var projModels = ServiceProvider.Instance.GetRequiredService<ProjectModels>();

            projModels.TargetFramework = tgtFW;
            projModels.AddProject( projFilePath ).Should().BeTrue();

            var result = projModels.Compile();

            if( !result )
                DisplayDiagnostics( projModels );

            result.Should().BeTrue();
        }

        [ Theory ]
        [ InlineData( "C:\\Programming\\RoslynUtils\\RoslynNetStandardTestLib\\RoslynNetStandardTestLib.csproj",
            "netstandard2.1" ) ]
        public void WalkerTest( string projFilePath, string tgtFWText )
        {
            TargetFramework.Create( tgtFWText, TargetFrameworkTextStyle.Simple, out var tgtFW ).Should().BeTrue();

            var projModels = ServiceProvider.Instance.GetRequiredService<ProjectModels>();

            projModels.TargetFramework = tgtFW;
            projModels.AddProject( projFilePath ).Should().BeTrue();

            var result = projModels.Compile();

            if( !result )
                DisplayDiagnostics( projModels );

            result.Should().BeTrue();

            projModels.GetCompilationResults( out var compResults ).Should().BeTrue();

            var walkers = ServiceProvider.Instance.GetRequiredService<SyntaxWalkers>();

            walkers.Traverse( compResults! ).Should().BeTrue();
        }

        private void DisplayDiagnostics( ProjectModels projModels )
        {
            foreach( var projModel in projModels )
            {
                foreach( var diagnostic in projModel.Diagnostics! )
                {
                    if( diagnostic.Location.SourceTree == null )
                    {
                        System.Diagnostics.Debug.WriteLine(
                            $"{projModel.ProjectName} [{diagnostic.Severity}({diagnostic.Id})] No source information available" );
                        continue;
                    }

                    var errorLines = diagnostic.Location.GetLineSpan();
                    var sourceText = diagnostic.Location.SourceTree!.GetText();

                    for( var lineNum = errorLines.StartLinePosition.Line;
                        lineNum <= errorLines.EndLinePosition.Line;
                        lineNum++ )
                    {
                        System.Diagnostics.Debug.WriteLine(
                            $"{projModel.ProjectName} [{diagnostic.Severity}({diagnostic.Id})] {diagnostic.GetMessage()} {sourceText.Lines[ lineNum ]}" );
                    }
                }
            }
        }
    }
}
