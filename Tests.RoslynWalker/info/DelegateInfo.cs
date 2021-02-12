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

using System;
using System.Collections.Generic;

namespace Tests.RoslynWalker
{
    public class DelegateInfo : NamedTypeInfo
    {
        public DelegateInfo( string name, Accessibility accessibility )
            : base( name, accessibility )
        {
        }

        public List<string> TypeArguments { get; } = new();
        public List<string> DelegateArguments { get; } = new();

        public static DelegateInfo Create( SourceLine srcLine )
        {
            var parts = srcLine.Line.Split( " ", StringSplitOptions.RemoveEmptyEntries );
            var text = parts.Length > 3 ? parts[ 3 ][ ..^1 ] : parts[ 2 ];

            var openParenLoc = text.IndexOf( "(", StringComparison.Ordinal );

            var retVal = new DelegateInfo( text[ ..openParenLoc ], srcLine.Accessibility );

            retVal.DelegateArguments.AddRange( SourceText.GetArgs( text ) );

            return retVal;
        }
    }
}