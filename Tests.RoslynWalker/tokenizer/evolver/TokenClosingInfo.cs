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
    public class TokenClosingInfo : TokenEvolutionInfo    
    {
        public TokenClosingInfo( 
            Token originalToken,
            TokenClosingAction closingAction = TokenClosingAction.DoNotClose, 
            string? revisedText = null 
            )
        : base(
            originalToken, 
            new TokenBase(originalToken.Type, revisedText ?? originalToken.Text), 
            TokenRelativePosition.Self, 
            closingAction)
        {
        }
    }

    public enum TokenRelativePosition
    {
        Child,
        Parent,
        Self
    }

    public class TokenEvolutionInfo
    {
        private readonly StringComparison _textComp;

        protected TokenEvolutionInfo( 
            Token originalToken, 
            TokenBase newToken,
            TokenRelativePosition relativePosition = TokenRelativePosition.Self,
            TokenClosingAction closingAction = TokenClosingAction.DoNotClose,
            StringComparison textComp = StringComparison.OrdinalIgnoreCase
            )
        {
            OriginalToken = originalToken;
            NewToken = newToken;

            RelativePosition = relativePosition;
            ClosingAction = closingAction;

            _textComp = textComp;
        }

        public Token OriginalToken { get; }
        public TokenBase NewToken { get; }

        public bool NeedsChange => ClosingAction != TokenClosingAction.DoNotClose
                                   || RelativePosition != TokenRelativePosition.Self
                                   || NewToken.CanAcceptText != OriginalToken.CanAcceptText
                                   || !NewToken.Text.Equals( OriginalToken.Text, _textComp )
                                   || NewToken.Type != OriginalToken.Type;

        public bool TextChanged => !NewToken.Text.Equals( OriginalToken.Text, _textComp );

        public TokenRelativePosition RelativePosition { get; }
        public TokenClosingAction ClosingAction { get; }
    }
}