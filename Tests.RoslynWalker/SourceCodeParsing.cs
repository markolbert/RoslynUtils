#region license

// Copyright 2021 Mark A. Olbert
// 
// This library or program 'Tests.RoslynWalker' is free software: you can redistribute it
// and/or modify it under the terms of the GNU General Public License as
// published by the Free Software Foundation, either version 3 of the License,
// or (at your option) any later version.
// 
// This library or program is distributed in the hope that it will be useful, but
// WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// General Public License for more details.
// 
// You should have received a copy of the GNU General Public License along with
// this library or program.  If not, see <https://www.gnu.org/licenses/>.

#endregion

using System.Linq;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Tests.RoslynWalker
{
    public class SourceCodeParsing
    {
        [ Theory ]
        [ InlineData( "C:\\Programming\\RoslynUtils\\RoslynNetStandardTestLib\\RoslynNetStandardTestLib.csproj" ) ]
        public void Parse( string projFilePath )
        {
            var namespaces = ServiceProvider.Instance.GetRequiredService<NamespaceCollection>();

            namespaces.ParseFile( projFilePath, out _ ).Should().BeTrue();

            namespaces.Count().Should().BeGreaterThan( 0 );

            namespaces.Namespaces
                .SelectMany( x => x.Classes )
                .Count()
                .Should().BeGreaterThan( 0 );

            namespaces.Namespaces
                .SelectMany(x => x.Interfaces)
                .Count()
                .Should().BeGreaterThan(0);
        }
    }
}