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
    internal class Comma : SpecialProcessor
    {
        public Comma( ITokenizer tokenizer )
            : base( ',', tokenizer )
        {
        }

        protected override bool IsValid()
        {
            switch( Tokenizer.ActiveToken.Type )
            {
                case TokenType.Array:
                    if( Tokenizer.NextCharIsWhitespace || Tokenizer.NextCharIsValid( ']' ) )
                        return true;

                    LogInvalidNextCharacter();
                    return false;

                case TokenType.Text:
                    return true;

                case TokenType.TypeArgument when Tokenizer.ActiveToken.Length > 0:
                    return true;

                default:
                    LogInvalidNextCharacter();
                    return false;
            }
        }

        protected override bool AddToText()
        {
            if( base.AddToText() )
                return true;

            switch( Tokenizer.ActiveToken.Type )
            {
                case TokenType.Array:
                    Tokenizer.ActiveToken.AddChar(Character);
                    return true;

                case TokenType.Text:
                    // commas close text tokens, except when they're the child of a preprocessor token
                    if( Tokenizer.ActiveToken.ParentTokenIs( TokenType.Preprocessor ) )
                        Tokenizer.ActiveToken.AddChar( Character );
                    else Tokenizer.CreateToken();

                    return true;

                case TokenType.TypeArgument:
                    Tokenizer.ActiveToken.AddChar( Character );
                    return true;

                default:
                    return false;
            }
        }
    }
}