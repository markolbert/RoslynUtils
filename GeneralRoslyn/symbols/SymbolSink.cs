#region license

// Copyright 2021 Mark A. Olbert
// 
// This library or program 'GeneralRoslyn' is free software: you can redistribute it
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
using J4JSoftware.Utilities;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn
{
    public class SymbolSink<TSymbol> : ISymbolSink<TSymbol>
        where TSymbol : ISymbol
    {
        protected SymbolSink(
            ActionsContext context,
            IJ4JLogger? logger
        )
        {
            Context = context;

            Logger = logger;
            Logger?.SetLoggedType( GetType() );
        }

        protected IJ4JLogger? Logger { get; }
        protected ISyntaxWalker? SyntaxWalker { get; private set; }
        protected ActionsContext Context { get; }

        public bool Initialized { get; private set; }

        public virtual bool OutputSymbol( ISyntaxWalker syntaxWalker, TSymbol symbol )
        {
            return Initialized;
        }

        public virtual bool SupportsSymbol( Type symbolType )
        {
            return typeof(TSymbol) == symbolType;
        }

        public virtual bool InitializeSink( ISyntaxWalker syntaxWalker )
        {
            SyntaxWalker = syntaxWalker;

            Initialized = true;

            return true;
        }

        public virtual bool FinalizeSink( ISyntaxWalker syntaxWalker )
        {
            return Initialized;
        }

        bool ISymbolSink.OutputSymbol( ISyntaxWalker syntaxWalker, ISymbol symbol )
        {
            if( symbol is TSymbol castSymbol )
                return OutputSymbol( syntaxWalker, castSymbol );

            Logger?.Error( "{0} is not a {1}", nameof(symbol), typeof(TSymbol) );

            return false;
        }
    }
}