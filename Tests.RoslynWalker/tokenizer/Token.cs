using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests.RoslynWalker
{
    public enum FieldQualifier
    {
        Static,
        Readonly
    }

    public enum MethodQualifier
    {
        Static,
        Virtual,
        Override,
        New
    }

    public enum ArgumentQualifier
    {
        In,
        Out,
        Ref
    }

    public enum PropertyMethod
    {
        Get,
        Set,
        Init
    }

    public static class TokenizerExtensions
    {
        public static bool IsEnumText<TEnum>( string text, out TEnum? result )
            where TEnum : struct, Enum
        {
            result = null;

            var enumText = Enum.GetNames<TEnum>()
                .FirstOrDefault( x => x.ToLower().Equals( text, StringComparison.Ordinal ) );

            if( string.IsNullOrEmpty( enumText ) )
                return false;

            result = Enum.Parse<TEnum>( enumText, false );

            return true;
        }

        public static int NumCharactersInText( string text, char toCheck ) => text.Count( x => x == toCheck );

        public static int NetDelimitersInText( string text, char opener, char closer ) =>
            NumCharactersInText( text, opener ) - NumCharactersInText( text, closer );

    }

    public class Token
    {
        private readonly StringBuilder _sb = new();
        private readonly List<Token> _children = new();

        private TokenType? _type;
        private bool _canAcceptText = true;

        public Token( Statement statement, TokenType type, Token? parent )
        {
            Statement = statement;
            Parent = parent;
            _type = type;
        }

        public TokenType Type
        {
            get
            {
                if( _type != null )
                    return _type.Value;

                if( _type == TokenType.Text )
                {
                    var text = _sb.ToString().Trim();

                    if( text.Length >= 2
                        && ( text.IndexOf( "//", StringComparison.OrdinalIgnoreCase ) == 0
                             || text.IndexOf( "/*", StringComparison.OrdinalIgnoreCase ) == 0 ) )
                        return TokenType.Comment;

                    if( Statement.Type == StatementType.Field
                        && TokenizerExtensions.IsEnumText<FieldQualifier>( text, out _ ) )
                        return TokenType.FieldQualifier;

                    if( Statement.Type == StatementType.Method
                        && TokenizerExtensions.IsEnumText<MethodQualifier>( text, out _ ) )
                        return TokenType.MethodQualifier;

                    if( (Parent?.Type == TokenType.TypeArgument || Statement.Type == StatementType.Method)
                        && TokenizerExtensions.IsEnumText<ArgumentQualifier>( text, out _ ) )
                        return TokenType.ArgumentQualifier;

                    if( text.Length < 2 || text[ 0 ] != '[' || text[ ^1 ] != ']' ) 
                        return TokenType.Text;

                    if( text.Length == 2 || text.All( x => x == ',' ) )
                        return TokenType.ArrayQualifier;

                    return Statement.Type == StatementType.Property ? TokenType.PropertyIndexer : TokenType.Text;
                }

                return TokenType.Undefined;
            }
        }

        public bool CanAcceptText => _canAcceptText;

        public Statement Statement { get; private set; }
        public Token? Parent { get; }

        public ReadOnlyCollection<Token> Children => _children.AsReadOnly();
        
        public string Text => _sb.ToString();

        public void AddChar( char toAdd )
        {
            if( !_canAcceptText )
                throw new ArgumentException( "Token is closed and can't be modified" );

            _sb.Append( toAdd );

            // special handling of text and comment nodes...
            switch( Type )
            {
                case TokenType.Text:
                    // text nodes starting with // or /* should be converted to comment nodes
                    ConvertCommentTextToCommentNode();
                    break;

                case TokenType.Comment:
                    // comment nodes ending with */ should have the */ stripped and their
                    // containing statement closed
                    CleanMultilineCommentNode( _sb.ToString() );
                    break;
            }
        }

        public Token AddChild( TokenType type )
        {
            var retVal = new Token( Statement, type, this );
            _children.Add( retVal );

            return retVal;
        }

        public void Close() =>_canAcceptText = false;

        private void ConvertCommentTextToCommentNode()
        {
            var text = _sb.ToString();

            var startLoc = text.IndexOf( "//", StringComparison.OrdinalIgnoreCase );

            if( startLoc < 0 )
                startLoc = text.IndexOf( "/*", StringComparison.CurrentCultureIgnoreCase );

            if( startLoc < 0 )
                return;

            _sb.Remove( startLoc, text.Length - startLoc );
            _type = TokenType.Comment;

            // it's possible for a "multi-line" comment to actually only be a single line
            CleanMultilineCommentNode( text );
        }

        private void CleanMultilineCommentNode( string text )
        {
            // if we're a multi-line comment, close the statement off
            // after the comment closing sequence (*/) is encountered
            if( Type != TokenType.Comment 
                || text.Length < 2 
                || !text[ ^2.. ].Equals( "*/", StringComparison.OrdinalIgnoreCase ) ) 
                return;

            _sb.Remove( _sb.Length - 2, 2 );
        }

        public void Clear() => _sb.Clear();
        public void Move( Statement statement ) => Statement = statement;

        //public int NumCharactersInText( char toCheck ) => Text.Count( x => x == toCheck );

        //public int NetDelimitersInText( char opener, char closer ) =>
        //    NumCharactersInText( opener ) - NumCharactersInText( closer );
    }
}
