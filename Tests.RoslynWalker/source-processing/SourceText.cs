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
using System.Linq;

namespace Tests.RoslynWalker
{
    public static class SourceText
    {
        public static List<string> GetArgs( string text )
        {
            var retVal = new List<string>();

            var closeParenLoc = text.IndexOf( ")", StringComparison.Ordinal );
            if( closeParenLoc < 0 )
                return retVal;

            var args = text[ ..^closeParenLoc ]
                .Split( "," )
                .Select( x => x.Trim() )
                .ToList();

            foreach( var arg in args )
            {
                var argParts = arg.Split( " " );
                retVal.Add( argParts.Last() );
            }

            return retVal;
        }

        public static List<string> GetTypeArgs( string text )
        {
            var findGreaterThan = text.IndexOf( ">", StringComparison.Ordinal );

            if( findGreaterThan < 0 )
                return new List<string>();

            var typeArgs = text[ ..^findGreaterThan ]
                .Split( "," )
                .Select( x => x.Trim() )
                .ToList();

            return typeArgs.Select( x =>
                {
                    var typeParts = x.Split( " ", StringSplitOptions.RemoveEmptyEntries );

                    return typeParts.Length == 1 ? typeParts[ 0 ] : typeParts[ ^1 ];
                } )
                .ToList();
        }
    }
}