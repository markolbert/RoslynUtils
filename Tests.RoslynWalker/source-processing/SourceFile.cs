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
    public class SourceFile : IEnumerable<Statement>
    {
        private readonly ITokenizer _tokenizer;

        private List<Statement>? _statements;

        public SourceFile( ITokenizer tokenizer )
        {
            _tokenizer = tokenizer;
        }

        public bool ParseFile( string srcPath )
        {
            if( !_tokenizer.Tokenize(srcPath, out var statements  ))
                return false;

            _statements = statements!;

            return true;
        }

        public IEnumerator<Statement> GetEnumerator()
        {
            foreach( var line in _statements ?? Enumerable.Empty<Statement>() )
            {
                yield return line;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}