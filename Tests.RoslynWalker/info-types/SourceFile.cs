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

        private int _position;
        private Stack<string?> _parentLines = new();

        public SourceFile( string srcPath )
        {
            _text = File.ReadAllText( srcPath );
            _textLen = _text.Length;

            RootBlock = CreateBlocks();
        }

        public LineBlock RootBlock { get; }

        private LineBlock CreateBlocks()
        {
            _position = 0;
            MoveNext();

            var block = new LineBlock( null );
            var retVal = block;

            var sb = new StringBuilder();

            while( _position < _textLen )
            {
                switch( CurrentChar )
                {
                    case ';':
                        block.AddLine(sb.ToString()  );
                        sb.Clear();

                        break;

                    case '{':
                        block = new LineBlock( block.CurrentLine );
                        break;

                    case '}':
                        block = block?.SourceLine?.LineBlock
                                ?? throw new NullReferenceException(
                                    $"Attempted to move to an undefined parent LineBlock" );
                        break;
                }

                MoveNext();
            }

            return retVal;
        }

        private char CurrentChar => _position < _textLen - 1 ? _text[ _position ] : _text[ ^1 ];

        private string GetFragment( int length = 1 )
        {
            length = length < 1 ? 1 : length;

            var lastPos = _position + length - 1;
            lastPos = lastPos >= _textLen - 1 ? _textLen - 1 : lastPos;

            return _text[ _position..lastPos ];
        }

        private void MoveNext()
        {
            // skip consecutive white spaces
            if( char.IsWhiteSpace( CurrentChar ) )
            {
                while( true )
                {
                    _position++;

                    if( _position >= _textLen )
                        return;

                    if( !char.IsWhiteSpace( CurrentChar ) )
                        break;
                }
            }

            // for single line comments, skip past EOL
            if( GetFragment(2).Equals("//", StringComparison.Ordinal) )
            {
                while( !GetFragment( Environment.NewLine.Length )
                           .Equals( Environment.NewLine, StringComparison.Ordinal )
                       && _position < _textLen )
                {
                    _position++;
                }

                if( _position < _textLen - 1 )
                    _position += Environment.NewLine.Length - 1;
            }

            // for multi line comments skip past closing */
            if( GetFragment( 2 ).Equals( "/*", StringComparison.Ordinal ) )
            {
                while( !GetFragment( 2 )
                           .Equals( "*/", StringComparison.Ordinal )
                       && _position < _textLen )
                {
                    _position++;
                }

                if( _position < _textLen - 1 )
                    _position++;
            }

            // skip white space and newlines
            while( true )
            {
                if( _position >= _textLen )
                    break;

                if( char.IsWhiteSpace( CurrentChar ) )
                    _position++;
                else
                {
                    if( GetFragment( Environment.NewLine.Length )
                        .Equals( Environment.NewLine, StringComparison.Ordinal ) )
                        _position += Environment.NewLine.Length;
                    else break;
                }
            }
        }

        public IEnumerator<SourceLine> GetEnumerator()
        {
            _parentLines.Clear();

            foreach( var line in EnumerateLineBlock( RootBlock ) )
            {
                yield return line;
            }
        }

        private IEnumerable<SourceLine> EnumerateLineBlock( LineBlock block )
        {
            foreach( var srcLine in block.Lines )
            {
                yield return srcLine;

                if( srcLine.ChildBlock == null ) 
                    continue;

                foreach( var childLine in EnumerateLineBlock(srcLine.ChildBlock) )
                {
                    yield return childLine;
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}