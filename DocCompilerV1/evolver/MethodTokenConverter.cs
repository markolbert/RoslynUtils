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

using System.Linq;

namespace J4JSoftware.DocCompiler
{
    public class MethodTokenConverter : TokenEvolver<TokenConversionInfo>
    {
        public override TokenConversionInfo? Matches( Token.Statement statement, out Token? originalToken )
        {
            base.Matches( statement, out var tempToken );
            originalToken = tempToken;
            var token = originalToken;

            if( originalToken == null
                || originalToken.Type != TokenType.ArgumentList )
                return null;

            // delegates look like methods but use a keyword (delegate), so if we 
            // find one we shouldn't try to convert it to a method (which is why
            // we return a default TokenConversionInfo object)
            if( statement.Tokens.Any(t=>t.Type == TokenType.DelegateQualifier))
                return null;

            // we identify methods based on their having an ArgumentList coming
            // after a Text token. This should avoid triggering on tuple return types
            // because by the time the method argument list is encountered the only
            // tokens preceding a tuple return type should be qualifier tokens
            var argListIdx = statement.Tokens.Where( t => ReferenceEquals( t, token ) )
                .Select( ( t, i ) => i )
                .FirstOrDefault();

            // shouldn't happen, but...
            if( argListIdx < 1 )
                return null;

            // if the preceding token isn't a text token (i.e., the method's name),
            // we're not a method. If it is, we are, and we need to change the 
            // argument list token's parent to be a method qualifier
            return statement.Tokens[ argListIdx - 1 ].Type == TokenType.Text
                ? new TokenConversionInfo( originalToken, TokenType.MethodName, relativePosition: TokenRelativePosition.Parent )
                : null;
        }
    }
}