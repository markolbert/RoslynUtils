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
    public class InterfaceInfo : NamedTypeInfo, ICodeElementTypeArguments
    {
        protected InterfaceInfo( string name, Accessibility accessibility )
            : base( name, accessibility )
        {
        }

        public List<MethodInfo> Methods { get; } = new();
        public List<PropertyInfo> Properties { get; } = new();
        public List<EventInfo> Events { get; } = new();

        public List<string> TypeArguments { get; } = new();

        public static InterfaceInfo Create( SourceLine srcLine )
        {
            var (name, typeArgs) = GetNameAndTypeArguments( srcLine.Line );

            var retVal = new InterfaceInfo( name, srcLine.Accessibility );
            retVal.TypeArguments.AddRange( typeArgs );

            return retVal;
        }

        public static (string name, List<string> typeArgs) GetNameAndTypeArguments( string line )
        {
            var parts = line.Split( " ", StringSplitOptions.RemoveEmptyEntries );

            var rawName = parts.Length > 3 ? string.Join( " ", parts[ 2.. ] ) : parts[ 2 ];

            var findColon = rawName.IndexOf( ":", StringComparison.Ordinal );
            if( findColon >= 0 )
                rawName = rawName[ ..( findColon - 1 ) ];

            var typeArgs = new List<string>();

            var findLessThan = rawName.IndexOf( "<", StringComparison.Ordinal );

            if( findLessThan < 0 )
                return ( rawName, typeArgs );

            return ( rawName[ ..findLessThan ].Trim(),
                SourceText.GetTypeArgs( rawName[ ( findLessThan + 1 ).. ].Trim() ) );
        }
    }
}