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
using System.Collections.ObjectModel;
using System.Linq;

namespace J4JSoftware.DocCompiler
{
    public class TokenCloser : TokenEvolver<TokenClosingInfo>
    {
        public TokenCloser(
            string closingText,
            TokenType? newType = null,
            Func<Token, bool>? includeToken = null
        )
            : base( includeToken )
        {
            ClosingText = closingText;
            NewType = newType;
        }

        public string ClosingText { get; }
        public TokenType? NewType { get; }

        public override bool Matches( TokenCollection tokenCollection, out TokenClosingInfo? result )
        {
            result = null;

            if( !base.Matches( tokenCollection, out _ ) )
                return false;

            if( !HasTrailingMatch( ActiveToken!, ClosingText ) )
                return false;

            result = new TokenClosingInfo( ActiveToken!,
                true,
                NewType,
                ActiveToken!.Text[ ..^ClosingText.Length ].Trim() );

            return true;
        }
    }
}