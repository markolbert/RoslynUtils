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
using System.Linq;
using System.Text;

namespace J4JSoftware.DocCompiler
{
    public class TokenSpawner : TokenEvolver<TokenSpawnInfo>
    {
        public TokenSpawner( 
            string spawnStart, 
            TokenType spawnedType,
            TokenRelativePosition relativePosition = TokenRelativePosition.Child
            )
        {
            SpawnStart = spawnStart;
            SpawnedType = spawnedType;
            RelativePosition = relativePosition;
        }

        public string SpawnStart {get;}
        public TokenType SpawnedType {get;}
        public TokenRelativePosition RelativePosition {get;}

        public override TokenSpawnInfo? Matches( Token.Statement statement, out Token? originalToken )
        {
            base.Matches( statement, out var tempToken );
            originalToken = tempToken;

            if( originalToken == null
                || originalToken.Type != TokenType.Text
                || !originalToken.CanAcceptText
                || string.IsNullOrEmpty( SpawnStart ) )
                return null;

            var origType = originalToken.Type;

            var location = originalToken.Text.IndexOf( SpawnStart, StringComparison.Ordinal );

            if( location < 0 )
                return null;

            var spawnText = originalToken.Text[ location.. ];
            var revisedText = originalToken.Text[ ..location ];

            return new TokenSpawnInfo( originalToken, RelativePosition, SpawnedType, spawnText, revisedText );
        }
    }
}