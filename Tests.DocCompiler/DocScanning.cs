using System;
using System.Collections.Generic;
using FluentAssertions;
using J4JSoftware.DocCompiler;
using Xunit;

namespace Tests.DocCompiler
{
    public class DocScanning
    {
        [ Theory ]
        //[ MemberData( nameof(TokenizerMemberData.GetTokenizerData), MemberType = typeof(TokenizerMemberData) ) ]
        [InlineData("C:\\Programming\\RoslynUtils\\TestLib\\DelegateClass.cs", true)]
        [InlineData("C:\\Programming\\RoslynUtils\\TestLib\\DelegateClassXXX.cs", false)]
        public void SingleFile( string filePath, bool success )
        {
            var docScanner = CompositionRoot.Default.DocScanner;

            docScanner.Scan( filePath ).Should().Be( success );
        }

        [Theory]
        [InlineData("C:\\Programming\\RoslynUtils\\TestLib\\TestLib.csproj", true)]
        public void Project( string projFilePath, bool success )
        {
            var docScanner = CompositionRoot.Default.DocScanner;

            docScanner.Scan( projFilePath ).Should().Be( success );
        }
    }
}
