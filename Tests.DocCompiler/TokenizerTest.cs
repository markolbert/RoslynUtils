using System;
using System.Collections.Generic;
using FluentAssertions;
using J4JSoftware.DocCompiler;
using Xunit;

namespace Tests.DocCompiler
{
    public class TokenizerTest
    {
        [ Theory ]
        //[ MemberData( nameof(TokenizerMemberData.GetTokenizerData), MemberType = typeof(TokenizerMemberData) ) ]
        [InlineData("C:\\Programming\\RoslynUtils\\TestLib\\DelegateClass.cs", true)]
        [InlineData("C:\\Programming\\RoslynUtils\\TestLib\\DelegateClassXXX.cs", false)]
        public void SingleFileParsing( string filePath, bool success )
        {
            var nodeCollector = CompositionRoot.Default.DocNodeCollector;

            nodeCollector.ParseSourceFile( filePath ).IsParsed.Should().Be( success );
        }

        [Theory]
        [InlineData("C:\\Programming\\RoslynUtils\\TestLib\\TestLib.csproj", true)]
        public void ProjectParsing( string projFilePath, bool success )
        {
            var nodeCollector = CompositionRoot.Default.DocNodeCollector;

            var parsedProject = nodeCollector.ParseProject( projFilePath );
            parsedProject.Should().NotBeNull();
            parsedProject!.IsParsed.Should().Be( success );
        }
    }
}
