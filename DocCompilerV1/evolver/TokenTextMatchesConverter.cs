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

namespace J4JSoftware.DocCompiler
{
    public class TokenTextMatchesConverter : TokenEvolver<TokenConversionInfo>
    {
        public TokenTextMatchesConverter(
            string toMatch,
            TokenType newType
        )
            : this( toMatch, newType, t=>true )
        {
        }

        public TokenTextMatchesConverter(
            string toMatch,
            TokenType newType,
            Func<Token, bool>? includeToken
        )
            : base( includeToken )
        {
            ToMatch = toMatch;
            NewType = newType;
        }

        public string ToMatch { get; }
        public TokenType NewType { get; }

        public override bool Matches( TokenCollection tokenCollection, out TokenConversionInfo? result )
        {
            result = null;

            if( !base.Matches( tokenCollection, out _ ) )
                return false;

            result = MatchesExactly( ActiveToken!, ToMatch )
                ? new TokenConversionInfo( ActiveToken!, NewType )
                : null;

            return result != null;
        }

        private bool MatchesExactly( Token token, string toMatch )
            => token.Text.Equals( toMatch, StringComparison.Ordinal );
    }
}