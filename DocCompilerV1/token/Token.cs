using System.Text;

namespace J4JSoftware.DocCompiler
{
    public class Token : TokenBase
    {
        private readonly StringBuilder _sb = new();

        internal Token(
            TokenCollection tokenCollection,
            TokenType type = TokenType.Text,
            string? initialText = null
        )
            : base( type )
        {
            Collection = tokenCollection;

            if( !string.IsNullOrEmpty( initialText ) )
                _sb.Append( initialText! );
        }

        public TokenCollection Collection { get; }

        public override string Text => _sb.ToString();
        public int Length => _sb.Length;

        internal void AddChar( char toAdd )
        {
            if( !CanAcceptText )
                return;

            _sb.Append( toAdd );
        }

        public void ReplaceText( string text )
        {
            _sb.Clear();
            _sb.Append( text );
        }
    }
}
