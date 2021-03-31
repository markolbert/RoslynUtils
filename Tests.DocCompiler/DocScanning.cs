using System;
using System.Collections.Generic;
using FluentAssertions;
using J4JSoftware.DocCompiler;
using Xunit;

namespace Tests.DocCompiler
{
    public class DocScanning
    {
        [Theory]
        [InlineData("C:\\Programming\\RoslynUtils\\TestLib\\TestLib.csproj", true, true)]
        public void Project( string projFilePath, bool scanSuccess, bool updateSuccess )
        {
            var docScanner = CompositionRoot.Default.DocScanner;
            var dbUpdater = CompositionRoot.Default.DbUpdater;

            docScanner.Scan( projFilePath ).Should().Be( scanSuccess );

            if( !scanSuccess )
                return;

            dbUpdater.UpdateDatabase( docScanner ).Should().Be( updateSuccess );
        }
    }
}
