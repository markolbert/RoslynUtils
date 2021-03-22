using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using J4JSoftware.Logging;
#pragma warning disable 8618

namespace J4JSoftware.DocCompiler
{
    public class Tokenizer : ITokenizer
    {
        public static char[] Whitespace = { ' ', '\t', '\r', '\n' };
        public static char[] EOL = { '\r', '\n' };

        private readonly CharacterProcessor _defaultProcessor;
        private readonly Dictionary<char, CharacterProcessor> _specials = new();

        public Tokenizer(
            IEnumerable<SpecialProcessor> specials,
            IJ4JLogger? logger )
        {
            Logger = logger;
            Logger?.SetLoggedType( GetType() );

            _defaultProcessor = new CharacterProcessor( this );

            foreach( var item in specials )
            {
                if( _specials.ContainsKey( item.Character ) )
                    throw new ArgumentException(
                        $"Attempting to add duplicate SpecialProcessor object ('{item.Character}')" );

                _specials.Add( item.Character, item );
            }

            // create initial token
            CreateToken();
        }

        public IJ4JLogger? Logger { get; }

        public List<IToken> Tokens { get; } = new();
        public Token ActiveToken { get; set; }

        public bool GetPriorToken( out IToken? result )
        {
            result = null;

            if( ActiveToken.GetPriorToken( out var temp ) )
                result = temp;

            return result != null;
        }

        public bool PriorTokenIs( TokenType priorType ) => ActiveToken.PriorTokenIs( priorType );

        public bool IsWhitespace( char toCheck ) => Whitespace.Any( x => x == toCheck );
        public bool IsEOL( char toCheck ) => EOL.Any( x => x == toCheck );
        public bool IsText( char toCheck ) => char.IsLetter( toCheck );

        public char CurrentChar { get; private set; } = char.MinValue;
        
        public char NextChar { get; private set; } = char.MinValue;
        public bool NextCharIsWhitespace => NextChar != char.MinValue && IsWhitespace( NextChar );
        public bool NextCharIsText => IsText( NextChar );
        public bool NextCharIsValid( params char[] allowed ) => allowed.Any( x => x == NextChar );

        public void CreateToken()
        {
            var newToken = new Token( this );
            Tokens.Add( newToken );

            ActiveToken = newToken;
        }

        public bool TokenizeText( string text )
        {
            var length = text.Length;

            for( var idx = 0; idx < length; idx++ )
            {
                CurrentChar = text[ idx ];
                NextChar = idx < ( length - 1 ) ? text[ idx + 1 ] : char.MinValue;

                var processor = _specials.ContainsKey( CurrentChar ) ? _specials[ CurrentChar ] : _defaultProcessor;

                if( processor.Process() ) 
                    continue;

                Logger?.Error( "Failed to process character '{0}' at index {1}", CurrentChar, idx );

                return false;
            }

            return true;
        }
    }
}
