using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using J4JSoftware.Logging;
using SQLitePCL;

namespace Tests.RoslynWalker
{
    public partial class Token : TokenBase
    {
        private readonly List<Token> _children = new();
        private readonly IJ4JLogger? _logger;

        private StringBuilder _sb = new();

        private Token(
            TokenCollection tokenCollection,
            Token? parent,
            TokenType type,
            string? initialText = null,
            IJ4JLogger? logger = null
        )
            : base( type )
        {
            ContainingTokenCollection = tokenCollection;
            Parent = parent;

            if( !string.IsNullOrEmpty( initialText ) )
                _sb.Append( initialText! );

            _logger = logger;
            _logger?.SetLoggedType( GetType() );
        }

        public TokenCollection ContainingTokenCollection { get; }
        public Token? Parent { get; }

        public ReadOnlyCollection<Token> Children => _children.AsReadOnly();

        public override string Text => _sb.ToString();
        public int Length => _sb.Length;

        private void AddChar( char toAdd )
        {
            if( !CanAcceptText )
                return;

            _sb.Append( toAdd );
        }

        private void AddChild( TokenType type = TokenType.Text, string? text = null ) =>
            _children.Add( new Token( ContainingTokenCollection, this, type, text ) );

        private void ReplaceText( string text )
        {
            _sb.Clear();
            _sb.Append( text );
        }
    }
}
