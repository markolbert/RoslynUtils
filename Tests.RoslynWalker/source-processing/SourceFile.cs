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
using System.Linq;
using System.Text;

namespace Tests.RoslynWalker
{
    public class SourceFile : IEnumerable<StatementLine>
    {
        private readonly string _text;
        private readonly int _textLen;
        private readonly Stack<string?> _parentLines = new();

        private int _position;

        public SourceFile( string srcPath )
        {
            // read the file and remove preprocessor lines
            var lines = File.ReadAllLines( srcPath );

            // delete embedded single line comments
            _text = RemoveSingleLineCommentsAndPreProcessorLines( lines );

            // convert multiple spaces to a single space
            _text = ReplaceAllText( _text, "  ", " " );

            // remove spacing around certain delimiting characters
            foreach( var toTrim in new string[] { "{", "}", "(", ")", "[", "]", ":" } )
            {
                _text = ReplaceAllText( _text, " " + toTrim, toTrim );
                _text = ReplaceAllText( _text, toTrim + " ", toTrim );
            }

            _textLen = _text.Length;

            RootBlock = new LineBlock( new BlockOpeningLine( string.Empty, null ) );
        }

        public LineBlock RootBlock { get; }

        private char CurrentChar => _position < _textLen - 1 ? _text[ _position ] : _text[ ^1 ];

        public IEnumerator<StatementLine> GetEnumerator()
        {
            _parentLines.Clear();

            foreach( var line in EnumerateLineBlock( RootBlock ) )
            {
                yield return line;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void ParseFile( ParserCollection parsers )
        {
            CreateBlocks();

            ParseBlock( RootBlock, parsers );
        }

        private void ParseBlock( LineBlock lineBlock, ParserCollection parsers )
        {
            foreach( var srcLine in lineBlock.Lines )
            {
                srcLine.Parse( parsers );

                if( srcLine is not BlockOpeningLine childLine )
                    continue;

                var parseableChild = srcLine.Elements?.FirstOrDefault();
                if( parseableChild == null )
                    continue;

                switch( parseableChild )
                {
                    case ClassInfo:
                    case InterfaceInfo:
                        ParseBlock(childLine.ChildBlock, parsers);
                        break;
                }
            }
        }

        private void CreateBlocks()
        {
            _position = -1;
            MoveNext();

            var block = RootBlock;
            var sb = new StringBuilder();

            while( _position < _textLen )
            {
                switch( CurrentChar )
                {
                    case ';':
                        block.AddStatement( sb.ToString() );
                        sb.Clear();

                        break;

                    case '{':
                        block = new LineBlock( block.AddBlockOpener( sb.ToString() ) );
                        sb.Clear();
                        break;

                    case '}':
                        block.AddBlockCloser();
                        sb.Clear();

                        block = block.ParentLine?.Parent
                                ?? throw new NullReferenceException(
                                    "Attempted to move to an undefined parent LineBlock" );
                        break;

                    default:
                        sb.Append( CurrentChar );
                        break;
                }

                MoveNext();
            }
        }

        private string RemoveSingleLineCommentsAndPreProcessorLines( string[] rawLines )
        {
            var lines = rawLines.Select( x => x.Trim() )
                .Where( x => x.Length > 0 && x[ 0 ] != '#'  )
                .ToList();

            for( var idx= 0; idx < lines.Count; idx++ )
            {
                var commentStart = lines[idx].IndexOf( "//", StringComparison.OrdinalIgnoreCase );
                if( commentStart < 0 )
                    continue;

                lines[ idx ] = lines[ idx ][ ..commentStart ];
            }

            // merge all lines together, separating each by a space
            return string.Join(" ", lines  )
                    .Replace( "\t", " " )
                    .Trim();
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

        private IEnumerable<StatementLine> EnumerateLineBlock( LineBlock block )
        {
            foreach( var srcLine in block.Lines )
            {
                yield return srcLine;

                if( srcLine is not BlockOpeningLine blockOpeningLine ) 
                    continue;

                foreach( var childLine in EnumerateLineBlock( blockOpeningLine.ChildBlock ) )
                {
                    yield return childLine;
                }
            }
        }
    }
}