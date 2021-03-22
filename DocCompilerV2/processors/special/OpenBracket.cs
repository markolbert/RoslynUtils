#region license

// Copyright 2021 Mark A. Olbert
// 
// This library or program 'DocCompiler' is free software: you can redistribute it
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

namespace J4JSoftware.DocCompiler
{
    internal class OpenBracket : SpecialProcessor
    {
        public OpenBracket( ITokenizer tokenizer )
            : base( '[', tokenizer )
        {
        }

        protected override bool IsValid()
        {
            if( Tokenizer.NextCharIsText || Tokenizer.NextCharIsWhitespace )
                return true;

            LogInvalidNextCharacter();

            return false;
        }

        protected override bool AddToText()
        {
            if( base.AddToText() )
                return true;

            if( Tokenizer.ActiveToken.Length == 0 )
            {
                Tokenizer.ActiveToken.Type = TokenType.Attribute;
                return true;
            }

            Tokenizer.ActiveToken.Type = Tokenizer.ActiveToken.Is( TokenType.This )
                ? TokenType.PropertyIndexer
                : TokenType.Array;

            Tokenizer.ActiveToken.AddChar( Character );

            return true;
        }
    }
}