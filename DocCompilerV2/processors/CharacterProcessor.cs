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
    public class CharacterProcessor
    {
        public CharacterProcessor( ITokenizer tokenizer )
        {
            Tokenizer = tokenizer;
        }

        public virtual bool Process() => IsValid() && AddToText();

        protected ITokenizer Tokenizer { get; }

        protected virtual bool IsValid() => true;
        
        protected virtual bool AddToText()
        {
            if( Tokenizer.ActiveToken.IsComment )
            {
                if( !Tokenizer.IsEOL(Tokenizer.CurrentChar))
                    Tokenizer.ActiveToken.AddChar( Tokenizer.CurrentChar );

                return true;
            }

            if( Tokenizer.ActiveToken.ParentTokenIs( TokenType.Preprocessor ) )
            {
                if( !Tokenizer.IsEOL(Tokenizer.CurrentChar))
                    Tokenizer.ActiveToken.AddChar( Tokenizer.CurrentChar );

                return true;
            }

            return false;
        }

        protected void LogInvalidNextCharacter() => Tokenizer.Logger?
            .Error( "Invalid character ('{0}') found after current character ('{1}')",
                Tokenizer.NextChar,
                Tokenizer.CurrentChar );

        protected void LogInvalidTokenType() =>
            Tokenizer.Logger?.Error( "Invalid TokenType '{0}' (current character is '{1}', next character is '{2}')",
                Tokenizer.ActiveToken.Type,
                Tokenizer.CurrentChar,
                Tokenizer.NextChar );
    }
}