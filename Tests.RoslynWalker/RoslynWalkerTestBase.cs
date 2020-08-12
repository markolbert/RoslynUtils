using System;
using System.Linq;
using FluentAssertions;
using J4JSoftware.Logging;
using J4JSoftware.Roslyn;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Tests.RoslynWalker
{
    public class RoslynWalkerTestBase
    {
        //[ Theory ]
        //[ InlineData( "C:\\Programming\\J4JLogging\\J4JLogging\\J4JLogging.csproj" ) ]
        //[ InlineData( "C:\\Programming\\J4JLogging\\ConsoleChannel\\ConsoleChannel.csproj" ) ]
        //public async void CompilationTest( string projFilePath )
        //{
        //    var ws = ServiceProvider.Instance.GetRequiredService<DocumentationWorkspace>();

        //    ws.AddProject( projFilePath ).Should().BeTrue();

        //    var result = await ws.Compile();
        //    result.Should().NotBeNull();
        //}

        [ Theory ]
        [ InlineData( "C:\\Programming\\RoslynUtils\\RoslynNetStandardTestLib\\RoslynNetStandardTestLib.csproj") ]
        public async void WalkerTest( string projFilePath )
        {
            var ws = ServiceProvider.Instance.GetRequiredService<DocumentationWorkspace>();

            ws.AddProject( projFilePath ).Should().BeTrue();

            var result = await ws.Compile();
            result.Should().NotBeNull();

            result!.Count.Should().BeGreaterThan( 0 );

            var walkers = ServiceProvider.Instance.GetRequiredService<ISyntaxWalkers>();

            walkers.Process( result ).Should().BeTrue();
        }
    }
}
