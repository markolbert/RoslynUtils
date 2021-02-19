﻿#region license

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
    public class DelegateInfo : ElementInfo, ITypeArguments, IArguments
    {
        public DelegateInfo()
            : base( ElementNature.Delegate )
        {
        }

        public List<string> TypeArguments { get; } = new();
        public List<string> Arguments { get; } = new();

        public override string FullName
        {
            get
            {
                var typeArgs = TypeArguments.Count > 0 ? $"<{string.Join( ", ", TypeArguments )}>" : string.Empty;
                var args = Arguments.Any() ? $"( {string.Join(", ", Arguments)} )" : "()";

                return $"{FullNameWithoutArguments}{typeArgs}{args}";
            }
        }

        //public static DelegateInfo Create( SourceLine srcLine )
        //{
        //    var parts = srcLine.Line.Split( " ", StringSplitOptions.RemoveEmptyEntries );
        //    var text = parts.Length > 3 ? parts[ 3 ][ ..^1 ] : parts[ 2 ];

        //    var openParenLoc = text.IndexOf( "(", StringComparison.Ordinal );

        //    var retVal = new DelegateInfo( text[ ..openParenLoc ], srcLine.Accessibility );

        //    retVal.Arguments.AddRange( SourceText.GetArgs( text ) );

        //    return retVal;
        //}
    }
}