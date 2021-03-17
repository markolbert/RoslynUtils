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
    public class TokenSpawner
    {
        protected TokenSpawner()
        {
        }

        protected TokenSpawnInfo SpawnToken( Token.Statement statement, TokenType type, bool spawnAsChild, params string[] starters )
        {
            var token = statement.GetActiveToken( true )!;

            if( token.Type != TokenType.Text )
                return new TokenSpawnInfo( token, TokenRelativePosition.Self );

            foreach( var starter in starters )
            {
                var location = token.Text.IndexOf( starter, StringComparison.OrdinalIgnoreCase );

                if( location >= 0 )
                    return new TokenSpawnInfo( token,
                        spawnAsChild ? TokenRelativePosition.Child : TokenRelativePosition.Self,
                        type,
                        token.Text[ ( location + starter.Length ).. ],
                        token.Text[ ..location ] );
            }

            return new TokenSpawnInfo( token, TokenRelativePosition.Self );
        }
    }
}