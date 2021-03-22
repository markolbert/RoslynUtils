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
    public class TokenTrailingModifier : TokenEvolver<TokenModificationInfo>
    {
        public TokenTrailingModifier(
            string toReplace,
            Func<Token, bool>? includeToken = null
        )
            : base( includeToken ?? (t => true) )
        {
            ToReplace = toReplace;
        }

        public TokenTrailingModifier(
            string toReplace,
            string replacementText,
            Func<Token, bool>? includeToken = null
        )
            : base( includeToken ?? (t => true) )
        {
            ToReplace = toReplace;
            ReplacementText = replacementText;
        }

        public string ToReplace {get;}
        public string? ReplacementText { get; }

        public override bool Matches( TokenCollection tokenCollection, out TokenModificationInfo? result )
        {
            result = null;

            if( !base.Matches( tokenCollection, out _ ) )
                return false;

            if( !HasTrailingMatch( ActiveToken!, ToReplace ) )
                return false;

            result = new TokenModificationInfo( ActiveToken!,
                $"{ActiveToken!.Text[ ..^ToReplace.Length ]}{ReplacementText ?? string.Empty}" );

            return true;
        }
    }
}