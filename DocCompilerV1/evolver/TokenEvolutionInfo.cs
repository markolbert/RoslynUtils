#region license

// Copyright 2021 Mark A. Olbert
// 
// This library or program 'DocCompiler' is free software: you can redistribute it
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
    public class TokenEvolutionInfo
    {
        private readonly StringComparison _textComp;

        protected TokenEvolutionInfo( 
            Token originalToken, 
            TokenBase newToken,
            StringComparison textComp = StringComparison.OrdinalIgnoreCase
        )
        {
            OriginalToken = originalToken;
            NewToken = newToken;

            _textComp = textComp;
        }

        public Token OriginalToken { get; }
        public TokenBase NewToken { get; }

        public bool NeedsChange => NewToken.CanAcceptText != OriginalToken.CanAcceptText
                                   || !NewToken.Text.Equals( OriginalToken.Text, _textComp )
                                   || NewToken.Type != OriginalToken.Type;

        public bool TextChanged => !NewToken.Text.Equals( OriginalToken.Text, _textComp );
    }
}