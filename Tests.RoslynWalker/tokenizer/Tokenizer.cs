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
using System.Collections.Generic;
using System.IO;
using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;
#pragma warning disable 8618

namespace Tests.RoslynWalker
{
    public class Tokenizer : ITokenizer
    {
        private enum IgnoredTextResult
        {
            Continue,
            AscendedToParent,
            Abort
        }

        private readonly IJ4JLogger? _logger;
        
        private List<Statement> _statements;
        private Statement _statement;
        //private bool _ignoreText;
        private string _text;
        private int _charNum;
        private int _ignoreTextStart;
        private int _netBraces;

        public Tokenizer( IJ4JLogger? logger )
        {
            _logger = logger;
            _logger?.SetLoggedType( GetType() );
        }

        public bool Tokenize( string srcPath, out List<Statement>? result )
        {
            result = null;

            if( !File.Exists( srcPath ) )
            {
                _logger?.Error<string>( "File '{0}' does not exist", srcPath );
                return false;
            }

            if( !Path.GetExtension( srcPath ).Equals( ".cs", StringComparison.OrdinalIgnoreCase ) )
            {
                _logger?.Error<string>( "File '{0}' is not a C# source file", srcPath );
                return false;
            }

            _text = File.ReadAllText( srcPath );
            _ignoreTextStart = -1;
            //_ignoreText = false;

            _statement = new Statement( null );
            _statements = new List<Statement> { _statement };

            _charNum = 0;

            //for( var charNum = 0; charNum < text.Length; charNum++)
            while( _charNum < _text.Length) 
            {
                if( _ignoreTextStart >= 0 )
                {
                    var endIgnore = SkipIgnoredText();

                    if( endIgnore >= _text.Length )
                        break;
                }

                // convert tab characters to spaces
                var curChar = _text[ _charNum ] == '\t' ? ' ' : _text[ _charNum ];

                //// we ignore text below certain statements, keeping track of 
                //// the net braces ({,}) we encounter and shifting out of ignore 
                //// mode once the net brace count goes to 0.
                //if( _ignoreText )
                //{
                //    switch( ProcessIgnoredText( curChar ) )
                //    {
                //        case IgnoredTextResult.Abort:
                //            return false;

                //        case IgnoredTextResult.AscendedToParent:
                //            _ignoreText = false;
                //            break;
                //    }
                //}
                //else
                //{
                    var okay = curChar switch
                    {
                        '\r' => ProcessCarriageReturnOrNewLine( curChar ),
                        '\n' => ProcessCarriageReturnOrNewLine( curChar ),
                        '{' => ProcessOpeningBrace(),
                        '}' => ProcessClosingBrace(),
                        ';' => ProcessSemicolon(),
                        '[' => ProcessOpeningBracket(),
                        ']' => ProcessClosingBracket(),
                        '<' => ProcessOpeningAngleBracket(),
                        '>' => ProcessClosingAngleBracket(),
                        '(' => ProcessOpeningParenthesis(),
                        ')' => ProcessClosingParenthesis(),
                        ',' => ProcessComma(),
                        ' ' => ProcessSpace(),
                        '=' => ProcessEqualsSign(),
                        '#' => ProcessHashSymbol(),
                        _ => ProcessRegularCharacter( curChar )
                    };

                    if( !okay )
                        return false;
                //}

                _charNum++;
            }

            result = _statements;

            return true;
        }

        private bool ProcessCarriageReturnOrNewLine( char toProcess )
        {
            // if no token is defined just return because we haven't yet encountered any meaningful text
            if( !TokenIsLive() )
                return true;

            if( toProcess != '\r' && toProcess != '\n' )
            {
                _logger?.Error(
                    "Trying to process something other than a carriage return or a newline in ProcessCarriageReturnOrNewLine()" );
                return false;
            }

            // carriage returns and new lines end a statement whose last token is a comment token
            // or if the statement is a preprocessor directive. Otherwise they're ignored
            if( _statement!.ActiveToken?.Type == TokenType.Comment )
            {
                _statement.Close();
                return true;
            }

            if( _statement.Type == StatementType.Preprocessor )
            {
                _statement.Close();
                return true;
            }

            return true;
        }

        private bool ProcessOpeningBrace()
        {
            if( !TokenIsLive() )
            {
                _statement.AddToken(new Token(_statement, TokenType.Text, null));
                return true;
            }

            return EndStatement( '{' );
        }

        private bool ProcessSemicolon()
        {
            if( !TokenIsLive() )
                return true;

            return EndStatement( ';' );
        }

        private bool EndStatement( char toProcess )
        {
            if( AddToComment( toProcess ) )
                return true;

            _statement.Close();

            // we ignore text "below" certain statement types,
            // and close some statements when an opening brace
            // is encountered (so subsequent statements are created
            // as children)
            switch( _statement.Type )
            {
                case StatementType.Namespace:
                case StatementType.Class:
                case StatementType.Interface:
                case StatementType.Struct:
                case StatementType.Using:
                    _ignoreTextStart = -1;
                    //_ignoreText = false;

                    break;

                default:
                    _ignoreTextStart = _charNum;
                    //_ignoreText = true;
                    break;
            }

            return true;
        }

        private bool ProcessClosingBrace()
        {
            // if no token is defined just return because we haven't yet encountered any meaningful text
            if( !TokenIsLive() )
                return true;

            if( AddToComment( '}' ) )
                return true;

            // should already be closed, but...
            _statement!.Close();

            if( _statement.Parent == null )
            {
                _statement = new Statement( null );
                _statements.Add( _statement );
            }
            else _statement = _statement.Parent;

            return true;
        }

        private bool ProcessOpeningBracket()
        {
            // a bracket signals the start of an attribute or the start of an array definition
            // provided it's not in a comment
            if( AddToComment( '[' ) )
                return true;

            // if there is no current token or it's an attribute a bracket
            // signals the start of another attribute
            if( _statement.ActiveToken == null 
                || _statement.ActiveToken.Type == TokenType.Attribute )
                CreateToken( TokenType.Attribute );
            else
            {
                // otherwise, a bracket should be part of an array definition
                // or a property indexer...but we won't no which until more tokenizing
                // takes place, so we simply assume it's always the start of a new
                // text token, and leave figuring out which it is until tokenizing
                // is done.
                CreateToken( TokenType.Text, '[' );
            }

            return true;
        }

        private bool ProcessClosingBracket()
        {
            if( !TokenIsLive( ']' ) )
                return false;

            var netBrackets = TokenizerExtensions.NetDelimitersInText(_statement.ActiveToken!.Text,'[', ']' );

            return _statement.ActiveToken.Type switch
            {
                TokenType.Comment => AppendCharacter( ']' ),
                TokenType.Attribute => netBrackets switch
                {
                    0 => CloseToken(),
                    _ => AppendCharacter( ']' )
                },
                _ => AppendCharacter( ']' )
            };
        }

        private bool ProcessOpeningAngleBracket()
        {
            if( !TokenIsLive( '<' ) )
                return false;

            return _statement.ActiveToken!.Type switch
            {
                TokenType.Comment => AppendCharacter( '<' ),
                _ => CreateChildToken( TokenType.TypeArgument )
            };
        }

        private bool ProcessClosingAngleBracket()
        {
            if( !TokenIsLive( '>' ) )
                return false;

            return _statement.ActiveToken!.Type switch
            {
                TokenType.Comment => AppendCharacter( '>' ),
                _ => CloseToken()
            };
        }

        private bool ProcessOpeningParenthesis()
        {
            if( !TokenIsLive( '(' ) )
                return false;

            return _statement.ActiveToken!.Type switch
            {
                TokenType.Comment => AppendCharacter( '(' ),
                TokenType.ArgumentList => CreateChildToken(TokenType.Cast),
                TokenType.Text => CreateToken(TokenType.ArgumentList),
                _ => UnexpectedTokenType('(')
            };
        }

        private bool ProcessClosingParenthesis()
        {
            if( !TokenIsLive( ')' ) )
                return false;

            var netParentheses = TokenizerExtensions.NetDelimitersInText( _statement.ActiveToken!.Text, '(', ')' );

            return netParentheses switch
            {
                0 => _statement.ActiveToken!.Type switch
                {
                    TokenType.Argument => CloseToken(),
                    TokenType.ArgumentList => CloseToken(),
                    _ => AppendCharacter( ')' )
                },
                _ => AppendCharacter( ')' )
            };
        }

        private bool ProcessComma()
        {
            if( !TokenIsLive(','))
                return false;

            var parentType = _statement.ActiveToken!.Parent?.Type ?? TokenType.Undefined;

            return _statement.ActiveToken.Type switch
            {
                TokenType.Argument => CloseToken(),
                TokenType.Text => parentType switch
                {
                    TokenType.TypeArgument => CloseToken(),
                    _ => AppendCharacter( ',' )
                },
                _ => AppendCharacter( ',' )
            };
        }

        private bool ProcessSpace()
        {
            // ignore leading spaces on statements and whenever
            // there's no token defined
            if( !TokenIsLive() )
                return true;

            // spaces signify the end of text tokens
            return _statement.ActiveToken!.Type switch
            {
                TokenType.Preprocessor => CloseToken(),
                TokenType.Text => CloseToken(),
                TokenType.Comment => AppendCharacter(' '),
                _ => true
            };
        }

        private bool ProcessEqualsSign()
        {
            if( !TokenIsLive())
                return false;

            return _statement.ActiveToken!.Type switch
            {
                TokenType.Argument => CreateChildToken( TokenType.Assignment ),
                _ => AppendCharacter( '=' )
            };
        }

        private bool ProcessHashSymbol()
        {
            if( TokenIsLive() )
                return _statement.ActiveToken!.Type switch
                {
                    TokenType.Comment => AppendCharacter( '#' ),
                    _ => UnexpectedTokenType( '#' )
                };

            CreateToken( TokenType.Preprocessor );
            
            return true;
        }

        private bool ProcessRegularCharacter( char toProcess )
        {
            // create a new text token if there isn't an existing token
            if( _statement.ActiveToken == null )
                return CreateToken( TokenType.Text, toProcess );

            // a closed preprocessor token is followed by a child text token
            if( !_statement.ActiveToken.CanAcceptText )
                return _statement.ActiveToken.Type switch
                {
                    TokenType.Preprocessor => CreateChildToken( TokenType.Text, toProcess ),
                    _ => CreateToken( TokenType.Text, toProcess )
                };

            return _statement.ActiveToken.Type switch
            {
                TokenType.ArgumentList => CreateChildToken( TokenType.Argument, toProcess ),
                TokenType.TypeArgument => CreateChildToken( TokenType.Text, toProcess ),
                _ => AppendCharacter( toProcess ),
            };
        }

        private int SkipIgnoredText()
        {
            var endIgnore = _ignoreTextStart;

            // have to ensure we skip past the first test, when the substring is blank...
            while( endIgnore < _text.Length
                   && ( _ignoreTextStart == endIgnore
                        || TokenizerExtensions.NetDelimitersInText( _text[ _ignoreTextStart..endIgnore ], '{', '}' ) !=
                        0 ) )
            {
                endIgnore++;
            }

            _ignoreTextStart = -1;

            return endIgnore + 1;
        }

        //private IgnoredTextResult ProcessIgnoredText( char toProcess )
        //{
        //    if( toProcess != '{' && toProcess != '}' )
        //        return IgnoredTextResult.Continue;

        //    if( toProcess == '{' )
        //        _netBraces++;
        //    else
        //    {
        //        if( toProcess == '}' )
        //            _netBraces--;
        //    }

        //    if( _netBraces != 0 )
        //        return IgnoredTextResult.Continue;

        //    if( _statement!.Parent == null )
        //    {
        //        _logger?.Error( "Trying to ascend to an undefined parent statement" );
        //        return IgnoredTextResult.Abort;
        //    }

        //    _statement = _statement!.Parent;

        //    return IgnoredTextResult.AscendedToParent;
        //}

        private bool UnexpectedTokenType( char curChar )
        {
            _logger?.Error("Encountered unexpected token type '{0}' while processing '{1}'", _statement.ActiveToken!.Type, curChar  );
            return false;
        }

        private bool AddToComment( char toProcess )
        {
            if( _statement.ActiveToken?.Type != TokenType.Comment )
                return false;

            AppendCharacter( toProcess );

            return true;
        }

        private bool AppendCharacter( char toAdd )
        {
            if( _statement.ActiveToken == null || (!_statement.ActiveToken?.CanAcceptText ?? false))
            {
                _logger?.Error("Trying to append '{0}' to undefined or closed token", toAdd  );
                return false;
            }

            _statement.ActiveToken!.AddChar( toAdd );

            return true;
        }

        private bool CreateToken( TokenType type, char toAdd = char.MinValue )
        {
            // close the currently active token because we're about to create a new one
            _statement.ActiveToken?.Close();

            // if the statement is closed create a new one
            if( !_statement.IsModifiable )
            {
                if( _statement.Parent == null )
                {
                    _statement = new Statement( null );
                    _statements.Add( _statement );
                }
                else _statement = new Statement( _statement );
            }

            _statement.AddToken(new Token( _statement, type, null ));

            if( toAdd != char.MinValue )
                _statement.ActiveToken!.AddChar( toAdd );

            return true;
        }

        private bool CloseToken()
        {
            _statement.ActiveToken?.Close();

            return true;
        }

        private bool CreateChildToken( TokenType type, char toAdd = char.MinValue )
        {
            _statement.ActiveToken!.AddChild( type );

            if( toAdd != char.MinValue )
                AppendCharacter( toAdd );

            return true;
        }

        private bool TokenIsLive( char curChar = char.MinValue )
        {
            if( _statement.ActiveToken != null && _statement.ActiveToken.CanAcceptText ) 
                return true;

            if( curChar != char.MinValue )
                _logger?.Error("Character '{0}' being processed on an undefined or closed token", curChar);
        
            return false;
        }
    }
}