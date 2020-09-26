using System;
using FluentAssertions;
using J4JSoftware.Roslyn;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Tests.RoslynWalker
{
    public class RoslynWalkerTestBase
    {
        [ Theory ]
        [ InlineData( "C:\\Programming\\RoslynUtils\\RoslynNetStandardTestLib\\RoslynNetStandardTestLib.csproj" ) ]
        public async void WalkerTest( string projFilePath )
        {
            var ws = ServiceProvider.Instance.GetRequiredService<DocumentationWorkspace>();

            ws.AddProject( projFilePath ).Should().BeTrue();

            var result = await ws.Compile();
            result.Should().NotBeNull();

            result!.Count.Should().BeGreaterThan( 0 );

            var context = ServiceProvider.Instance.GetRequiredService<ExecutionContext>();

            context.StopOnFirstError = true;

            var walkers = ServiceProvider.Instance.GetRequiredService<SyntaxWalkers>();

            walkers.Process(result).Should().BeTrue();
        }
    }
}
