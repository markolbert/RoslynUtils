#region license

// Copyright 2021 Mark A. Olbert
// 
// This library or program 'RoslynWalker' is free software: you can redistribute it
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
using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn
{
    public class DefaultSymbolSink : IDefaultSymbolSink
    {
        private readonly IJ4JLogger _logger;

        public DefaultSymbolSink(
            WalkerContext context,
            IJ4JLogger logger
        )
        {
            ExecutionContext = context;

            _logger = logger;
            _logger.SetLoggedType( GetType() );
        }

        public WalkerContext ExecutionContext { get; }

        // even though we support all ISymbols we deny it because we don't want
        // to be selected before a non-default sink can be selected
        public bool SupportsSymbol( Type symbolType )
        {
            return false;
        }

        public bool InitializeSink( ISyntaxWalker syntaxWalker )
        {
            return true;
        }

        public bool FinalizeSink( ISyntaxWalker syntaxWalker )
        {
            return true;
        }

        public bool OutputSymbol( ISyntaxWalker syntaxWalker, ISymbol symbol )
        {
            _logger.Information<string>( "Processed a {0}", symbol.ToDisplayString() );

            return true;
        }
    }
}