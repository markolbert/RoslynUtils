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
    public class TokenModifier
    {
        protected TokenModifier()
        {
        }

        protected TokenModificationInfo RemoveFromEnd( Token.Statement statement, params string[] closers )
        {
            var token = statement.GetActiveToken( true )!;

            if( !token.CanAcceptText )
                return new TokenModificationInfo(token);

            var location = -1;

            foreach( string closer in closers )
            {
                location = token.Text.LastIndexOf( closer, StringComparison.Ordinal );

                if( location >= 0 )
                    break;
            }

            return location < 0 
                ? new TokenModificationInfo(token) 
                : new TokenModificationInfo( token, token.Text[ location..^location ] );
        }
    }
}