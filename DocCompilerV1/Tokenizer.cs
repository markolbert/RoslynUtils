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

#pragma warning disable 8618

namespace J4JSoftware.DocCompiler
{
    public class Tokenizer : ITokenizer
    {
        private readonly IActiveTokenEvolver _evolver;

        private readonly Func<IJ4JLogger>? _loggerFactory;
        private readonly IJ4JLogger? _logger;

        public Tokenizer( 
            IActiveTokenEvolver evolver,
            Func<IJ4JLogger>? loggerFactory
            )
        {
            _evolver = evolver;

            _loggerFactory = loggerFactory;
            _logger = _loggerFactory?.Invoke();
            _logger?.SetLoggedType( GetType() );
        }

        public bool TokenizeFile( string srcPath, out TokenCollection? result )
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

            var text = File.ReadAllText( srcPath );

            if( !TokenizeText( text, out var temp ) )
                return false;

            result = temp;

            return true;
        }

        public bool TokenizeText( string text, out TokenCollection? result )
        {
            result = new TokenCollection( _evolver, _loggerFactory?.Invoke() );

            var ignoreTextStart = -1;

            foreach( var curChar in text )
            {
                if( ignoreTextStart >= 0 )
                {
                    var endIgnore = FindEndOfIgnoredText( text, ignoreTextStart );

                    if( endIgnore >= text.Length )
                        break;
                }

                if( !result.AddChar( curChar ) )
                    return false;

                // see if we're about to drill down further than we want to go
                if( !result.IsModifiable )
                    ignoreTextStart = result.InTokenTypes( TokenizerExtensions.ContainerTypeTokens ) ? -1 : curChar;
            }

            return true;
        }

        private int FindEndOfIgnoredText( string text, int ignoreTextStart )
        {
            var endIgnore = ignoreTextStart;

            // have to ensure we skip past the first test, when the substring is blank...
            while( endIgnore < text.Length
                   && ( ignoreTextStart == endIgnore
                        || TokenizerExtensions.NetDelimitersInText( text[ ignoreTextStart..endIgnore ], '{', '}' ) !=
                        0 ) )
            {
                endIgnore++;
            }

            return endIgnore + 1;
        }
    }
}