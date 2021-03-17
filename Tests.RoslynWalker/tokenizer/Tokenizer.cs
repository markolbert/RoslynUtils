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
using Microsoft.EntityFrameworkCore.Metadata.Conventions;

#pragma warning disable 8618

namespace Tests.RoslynWalker
{
    public class Tokenizer : ITokenizer
    {
        private readonly ITokenCollectionFactory _tokenCollectionFactory;
        private readonly IJ4JLogger? _logger;
        
        private Token.TokenCollection? _statement;

        public Tokenizer( 
            ITokenCollectionFactory tokenCollectionFactory,
            IJ4JLogger? logger
            )
        {
            _tokenCollectionFactory = tokenCollectionFactory;

            _logger = logger;
            _logger?.SetLoggedType( GetType() );
        }

        public bool Tokenize( string srcPath, out List<Token.TokenCollection>? result )
        {
            result = new List<Token.TokenCollection>();

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
            var ignoreTextStart = -1;

            foreach( var curChar in text )
            {
                // if tokenCollection is closed or non-existent, create a new one
                if( _statement == null )
                {
                    _statement = _tokenCollectionFactory.CreateTokenCollection();
                    result.Add( _statement );
                }
                else
                {
                    if( !_statement.IsModifiable )
                        _statement = _statement.AddChild();
                }

                if( ignoreTextStart >= 0 )
                {
                    var endIgnore = FindEndOfIgnoredText( text, ignoreTextStart );

                    if( endIgnore >= text.Length )
                        break;
                }

                if( !_statement.AddChar( curChar ) )
                    return false;

                // see if we're about to drill down further than we want to go
                if( !_statement.IsModifiable )
                    ignoreTextStart = _statement.Type switch
                    {
                        StatementType.Namespace => -1,
                        StatementType.Class => -1,
                        StatementType.Interface => -1,
                        StatementType.Struct => -1,
                        StatementType.Record => -1,
                        StatementType.Comment => -1,
                        _ => curChar
                    };
            }

            return true;
        }

        //public void OnStatementClosed()
        //{
        //    if( _statement == null )
        //        return;

        //    _statement = _statement.Parent;
        //}

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