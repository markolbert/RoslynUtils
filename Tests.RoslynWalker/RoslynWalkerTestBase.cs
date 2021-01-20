using System;
using FluentAssertions;
using J4JSoftware.Roslyn;
using J4JSoftware.Utilities;
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

            var context = ServiceProvider.Instance.GetRequiredService<ActionsContext>();

            context.StopOnFirstError = true;

            var walker = ServiceProvider.Instance.GetRequiredService<ISingleWalker>();
            walker.Process( result ).Should().BeTrue();

            //var walkers = ServiceProvider.Instance.GetRequiredService<SyntaxWalkers>();

            //walkers.Process(result).Should().BeTrue();
        }
    }
}
