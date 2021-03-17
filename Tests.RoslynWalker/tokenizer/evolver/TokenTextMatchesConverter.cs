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

namespace Tests.RoslynWalker
{
    public class TokenTextMatchesConverter : TokenEvolver<TokenConversionInfo>
    {
        public TokenTextMatchesConverter(
            string toMatch,
            TokenType newType,
            TokenRelativePosition relativePosition = TokenRelativePosition.Self
        )
            : this( toMatch, newType, t=>true, s=>true, relativePosition )
        {
        }

        public TokenTextMatchesConverter(
            string toMatch,
            TokenType newType,
            Func<TokenType, bool> includeToken,
            Func<Token.TokenCollection, bool>? includeStatement = null,
            TokenRelativePosition relativePosition = TokenRelativePosition.Self
        )
            : base( includeToken, includeStatement )
        {
            ToMatch = toMatch;
            NewType = newType;
            RelativePosition = relativePosition;
        }

        public string ToMatch { get; }
        public TokenType NewType { get; }
        public TokenRelativePosition RelativePosition { get; }

        public override bool Matches( Token.TokenCollection tokenCollection, out TokenConversionInfo? result )
        {
            result = null;

            if( !base.Matches( tokenCollection, out _ ) )
                return false;

            result = new TokenConversionInfo( ActiveToken!, NewType, relativePosition: RelativePosition );

            return true;
        }

        private bool MatchesExactly( Token token, string toMatch )
            => token.Text.Equals( toMatch, StringComparison.Ordinal );


    }
}