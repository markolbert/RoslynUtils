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
    public abstract class TokenEvolver<TResult> : ITokenEvolver
        where TResult : TokenEvolutionInfo
    {
        protected TokenEvolver(
            Func<TokenType, bool> includeToken,
            Func<Token.TokenCollection, bool>? includeStatement = null
            )
        {
            IncludeToken = includeToken;
            IncludeStatement = includeStatement ?? (s => true);
        }

        protected Token? ActiveToken { get; private set; }

        public Type ResultType => typeof(TResult);
        public Func<TokenType, bool> IncludeToken { get; }
        public Func<Token.TokenCollection, bool> IncludeStatement { get; }

        public virtual bool Matches( Token.TokenCollection tokenCollection, out TResult? result )
        {
            result = null;

            ActiveToken = tokenCollection.GetActiveToken();

            return ActiveToken != null && IncludeStatement( tokenCollection ) && IncludeToken( ActiveToken.Type );
        }

        protected bool HasTrailingMatch( Token token, string toMatch )
        {
            if( token.Length == 0 || token.Length < toMatch.Length )
                return false;

            return token.Text[ ^( toMatch.Length ).. ].Equals( toMatch, StringComparison.Ordinal );
        }

        bool ITokenEvolver.Matches( Token.TokenCollection tokenCollection, out TokenEvolutionInfo? result )
        {
            result = Matches( tokenCollection, out var innerResult ) ? innerResult : null;

            return result != null;
        }
    }
}