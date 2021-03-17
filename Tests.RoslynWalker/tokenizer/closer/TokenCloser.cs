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
using System.Text;

namespace Tests.RoslynWalker
{
    public class TokenCloser : ITokenCloser
    {
        public TokenClosingInfo CloseActiveToken( Token.Statement statement )
        {
            var token = statement.GetActiveToken( true )!;

            if( token.Length == 0 )
                return new TokenClosingInfo(token);

            switch( token.Type )
            {
                case TokenType.SingleLineComment:
                    return FoundEndMatch( statement, TokenClosingAction.CloseTokenAndStatement, "\r", "\n" );

                case TokenType.MultiLineComment:
                    return FoundEndMatch( statement, TokenClosingAction.CloseTokenAndStatement, "*/" );

                default:
                    var retVal = FoundEndMatch( statement, 
                        TokenClosingAction.CloseToken, 
                        "\r", "\n", "\t", ":", " ", ")" );

                    return retVal.ClosingAction != TokenClosingAction.DoNotClose
                        ? retVal
                        : FoundEndMatch( statement, TokenClosingAction.CloseTokenAndStatement, "{", "}" );
            }
        }

        private TokenClosingInfo FoundEndMatch( Token.Statement statement, TokenClosingAction toReturn, params string[] matches )
        {
            var token = statement.GetActiveToken( true )!;

            if( token.Length == 0)
                return new TokenClosingInfo(token);

            foreach( var match in matches )
            {
                if( token.Length <= match.Length 
                    || !token.Text[ ^( match.Length ).. ].Equals( match, StringComparison.OrdinalIgnoreCase ) ) 
                    continue;

                return new TokenClosingInfo( token, toReturn, token.Text[ ..^match.Length ] );
            }

            return new TokenClosingInfo(token);
        }
    }
}