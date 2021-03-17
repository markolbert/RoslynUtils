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
    public class TokenLeadingConverter : TokenEvolver<TokenConversionInfo>
    {
        public TokenLeadingConverter(
            string toMatch,
            TokenType newType,
            bool removeMatchedText = true
        )
            : this( toMatch, newType, t=>true, s=>true, removeMatchedText )
        {
        }

        public TokenLeadingConverter(
            string toMatch,
            TokenType newType,
            Func<TokenType, bool> includeToken,
            Func<Token.TokenCollection, bool>? includeStatement = null,
            bool removeMatchedText = true
        )
            : base( includeToken, includeStatement )
        {
            ToMatch = toMatch;
            NewType = newType;
            RemoveMatchedText = removeMatchedText;
        }

        public string ToMatch { get; }
        public TokenType NewType { get; }
        public bool RemoveMatchedText { get; }

        public override bool Matches( Token.TokenCollection tokenCollection, out TokenConversionInfo? result )
        {
            result = null;

            if( !base.Matches( tokenCollection, out _ ) )
                return false;

            result = RemoveMatchedText 
                ? new TokenConversionInfo( ActiveToken!, NewType, string.Empty ) 
                : new TokenConversionInfo( ActiveToken!, NewType );

            return true;
        }

        private bool HasLeadingMatch( Token token, string toMatch )
            => token.Text.IndexOf( toMatch, StringComparison.Ordinal ) == 0;

    }
}