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
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Tests.RoslynWalker
{
    public class SourceFile : IEnumerable<SourceLine>
    {
        private readonly string _text;
        private readonly int _textLen;
        private readonly Stack<string?> _parentLines = new();

        private int _position;

        public SourceFile( string srcPath )
        {
            _text = File.ReadAllText( srcPath )
                .Replace( "\t", " " );

            _text = RemoveSingleLineComments( _text );
            _text = _text.Replace( Environment.NewLine, " " );
            _text = ReplaceAllText( _text, "  ", " " );
            _text = ReplaceAllText( _text, " {", "{" );
            _text = ReplaceAllText( _text, "{ ", "{" );
            _text = ReplaceAllText( _text, "} ", "}" );
            _text = ReplaceAllText( _text, " }", "}" );
            _text = ReplaceAllText( _text, "( ", "(" );
            _text = ReplaceAllText( _text, " )", ")" );
            _text = ReplaceAllText( _text, " [", "[" );
            _text = ReplaceAllText( _text, "[ ", "[" );
            _text = ReplaceAllText( _text, " ]", "]" );
            _text = ReplaceAllText( _text, "] ", "]" );

            _textLen = _text.Length;

            RootBlock = CreateBlocks();
        }

        public LineBlock RootBlock { get; }

        private char CurrentChar => _position < _textLen - 1 ? _text[ _position ] : _text[ ^1 ];

        public IEnumerator<SourceLine> GetEnumerator()
        {
            _parentLines.Clear();

            foreach( var line in EnumerateLineBlock( RootBlock ) ) yield return line;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private LineBlock CreateBlocks()
        {
            _position = -1;
            MoveNext();

            var block = new LineBlock( null );
            var retVal = block;

            var sb = new StringBuilder();

            while( _position < _textLen )
            {
                switch( CurrentChar )
                {
                    case ';':
                        block.AddLine( sb.ToString() );
                        sb.Clear();

                        break;

                    case '{':
                        block.AddLine( sb.ToString() );
                        sb.Clear();

                        block = new LineBlock( block.CurrentLine );
                        break;

                    case '}':
                        block.AddLine( sb.ToString() );
                        sb.Clear();

                        block = block?.SourceLine?.LineBlock
                                ?? throw new NullReferenceException(
                                    "Attempted to move to an undefined parent LineBlock" );
                        break;

                    default:
                        sb.Append( CurrentChar );
                        break;
                }

                MoveNext();
            }

            return retVal;
        }

        private string RemoveSingleLineComments( string text )
        {
            var newLineLength = Environment.NewLine.Length;
            var eol = -newLineLength;
            var sb = new StringBuilder();

            while( true )
            {
                var commentStart = text.IndexOf( "//", eol + newLineLength, StringComparison.OrdinalIgnoreCase );

                if( commentStart < 0 )
                {
                    sb.Append( text[ ( eol + newLineLength ).. ] );
                    break;
                }

                sb.Append( text[ ( eol + newLineLength )..( commentStart - 1 ) ] );

                eol = text.IndexOf( Environment.NewLine, commentStart, StringComparison.OrdinalIgnoreCase );

                if( eol < 0 )
                    break;
            }

            return sb.ToString();
        }

        private string ReplaceAllText( string text, string toFind, string replacement )
        {
            while( text.IndexOf( toFind, StringComparison.OrdinalIgnoreCase ) >= 0 )
                text = text.Replace( toFind, replacement );

            return text;
        }

        private void MoveNext()
        {
            _position++;

            // for multi line comments skip past closing */
            var nextTwoChars = _position < _textLen - 2
                ? _text[ _position..( _position + 1 ) ]
                : _text[ _position.._position ];

            if( !nextTwoChars.Equals( "/*", StringComparison.Ordinal ) )
                return;

            var endOfComment = _text.IndexOf( "*/", _position, StringComparison.Ordinal );

            if( endOfComment < 0 )
                _position = _textLen;
            else _position += endOfComment - _position + 2;
        }

        private IEnumerable<SourceLine> EnumerateLineBlock( LineBlock block )
        {
            foreach( var srcLine in block.Lines )
            {
                yield return srcLine;

                if( srcLine.ChildBlock == null )
                    continue;

                foreach( var childLine in EnumerateLineBlock( srcLine.ChildBlock ) ) yield return childLine;
            }
        }
    }
}