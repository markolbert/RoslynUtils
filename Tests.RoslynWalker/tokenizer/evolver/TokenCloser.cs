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
using System.Text;

namespace Tests.RoslynWalker
{
    public class TokenCloser : TokenEvolver<TokenClosingInfo>
    {
        public override TokenClosingInfo? Matches( Token.Statement statement, out Token? originalToken )
        {
            base.Matches( statement, out originalToken );

            if( originalToken == null )
                return null;

            return originalToken.Type switch
            {
                TokenType.SingleLineComment => MatchOn( originalToken, TokenClosingAction.CloseToken, "\r", "\n" ),
                TokenType.MultiLineComment => MatchOn( originalToken, TokenClosingAction.CloseToken, "*/" ),
                TokenType.XmlComment => MatchOn( originalToken, TokenClosingAction.CloseToken, "*/" ),

                _ => MatchEndingAndNamedTypeClosure( originalToken, 
                    TokenClosingAction.CloseToken, 
                    "\r", "\n", "\t", ":", " ", ")", "," )
            };
        }

        private TokenClosingInfo? MatchOn( Token token, TokenClosingAction closingAction, params string[] matches )
        {
            foreach( var match in matches )
            {
                if( !HasTrailingMatch( token, match ) )
                    continue;

                return new TokenClosingInfo( token, TokenClosingAction.CloseToken, token.Text[ ..^match.Length ] );
            }

            return null;
        }

        private TokenClosingInfo? MatchEndingAndNamedTypeClosure( Token token, TokenClosingAction closingAction, params string[] matches )
        {
            var endingInfo = MatchOn( token, closingAction, matches );

            return endingInfo ?? MatchOn( token, TokenClosingAction.CloseTokenAndStatement, "{", "}" );
        }
    }
}