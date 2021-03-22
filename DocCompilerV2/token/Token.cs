using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace J4JSoftware.DocCompiler
{
    public class Token : IToken
    {
        private readonly StringBuilder _sb = new();

        public Token( ITokenizer tokenizer, ITokenCollection belongsTo )
        {
            Tokenizer = tokenizer;
            BelongsTo = belongsTo;
            Type = TokenType.Text;
        }

        public Token( ITokenizer tokenizer )
        {
            Tokenizer = tokenizer;
            BelongsTo = tokenizer;
            Type = TokenType.Text;
        }

        private Token( Token parent )
        {
            Tokenizer = parent.Tokenizer;
            BelongsTo = parent;
            Type = TokenType.Text;
        }

        public List<IToken> Tokens { get; } = new();
        public ITokenizer Tokenizer { get; }
        public ITokenCollection BelongsTo { get; }
        public TokenType Type { get; set; }

        public string Text => _sb.ToString();
        public int Length => _sb.Length;
        public void AddChar( char toAdd ) => _sb.Append( toAdd );

        public void CreateChild()
        {
            var newToken = new Token( this );
            Tokens.Add( newToken );

            Tokenizer.ActiveToken = newToken;
        }

        public bool Is( params TokenType[] types ) => types.Any( t => t == Type );

        public bool IsComment => Is( TokenType.SingleLineComment,
            TokenType.MultiLineComment,
            TokenType.XmlComment );

        public bool GetPriorToken( out IToken? result )
        {
            result = null;

            var tokenIdx = BelongsTo.Tokens.FindIndex( x => ReferenceEquals( x, this ) );

            if( tokenIdx < 1 )
                return false;

            result = BelongsTo.Tokens[ tokenIdx - 1 ];

            return true;
        }

        public bool PriorTokenIs( params TokenType[] priorTypes ) =>
            GetPriorToken( out var token ) && token!.ParentTokenIs( priorTypes );

        public bool GetParentToken( out IToken? result )
        {
            result = null;

            if( BelongsTo is IToken parent )
                result = parent;

            return result != null;
        }

        public bool ParentTokenIs( params TokenType[] parentTypes ) =>
            GetParentToken( out var token ) && parentTypes.Any( x => x == token!.Type );
    }
}
