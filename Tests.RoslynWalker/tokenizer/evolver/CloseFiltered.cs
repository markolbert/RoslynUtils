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
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Tests.RoslynWalker
{
    public class CloseFiltered : TokenEvolver<TokenClosingInfo>
    {
        private readonly List<string> _closers = new();

        public CloseFiltered(
            bool closeStatement,
            Func<TokenType, bool> includeToken,
            Func<Token.TokenCollection, bool>? includeStatement = null
        )
            : base( includeToken, includeStatement )
        {
            CloseStatement = closeStatement;
        }

        public CloseFiltered(
            string closingText,
            bool closeStatement,
            Func<TokenType, bool> includeToken,
            Func<Token.TokenCollection, bool>? includeStatement = null
        )
            : this( closeStatement, includeToken, includeStatement )
        {
            _closers.Add( closingText );
        }

        public ReadOnlyCollection<string> Closers => _closers.AsReadOnly();

        public CloseFiltered AddClosers( params string[] closers )
        {
            foreach( var closer in closers )
            {
                if( _closers.Any( x => !x.Equals( closer, StringComparison.Ordinal ) ) )
                    _closers.Add( closer );
            }

            return this;
        }

        public bool CloseStatement { get; }

        public override bool Matches( Token.TokenCollection tokenCollection, out TokenClosingInfo? result )
        {
            result = null;

            if( !base.Matches( tokenCollection, out _ ) )
                return false;

            foreach( var closer in _closers )
            {
                if( !HasTrailingMatch( ActiveToken!, closer ) )
                    continue;

                result = new TokenClosingInfo( ActiveToken!,
                    CloseStatement ? TokenClosingAction.CloseTokenAndStatement : TokenClosingAction.CloseToken, 
                    ActiveToken!.Text[ ..^closer.Length ]);

                return true;
            }

            return false;
        }
    }
}