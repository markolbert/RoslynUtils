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

namespace J4JSoftware.DocCompiler
{
    public abstract class TokenEvolver<TResult> : ITokenEvolver
        where TResult : TokenEvolutionInfo
    {
        protected TokenEvolver( Func<Token, bool>? includeToken )
        {
            IncludeToken = includeToken ?? ( t => true );
        }

        protected Token? ActiveToken { get; private set; }

        public Type ResultType => typeof(TResult);
        public Func<Token, bool> IncludeToken { get; }

        public virtual bool Matches( TokenCollection tokenCollection, out TResult? result )
        {
            result = null;

            ActiveToken = tokenCollection.GetActiveToken();

            return ActiveToken != null && IncludeToken( ActiveToken );
        }

        protected bool HasTrailingMatch( Token token, string toMatch )
        {
            if( token.Length == 0 || token.Length < toMatch.Length )
                return false;

            return token.Text[ ^( toMatch.Length ).. ].Equals( toMatch, StringComparison.Ordinal );
        }

        bool ITokenEvolver.Matches( TokenCollection tokenCollection, out TokenEvolutionInfo? result )
        {
            result = Matches( tokenCollection, out var innerResult ) ? innerResult : null;

            return result != null;
        }
    }
}