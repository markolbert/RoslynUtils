#region license

// Copyright 2021 Mark A. Olbert
// 
// This library or program 'RoslynDb' is free software: you can redistribute it
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

using System.Collections.Generic;
using System.Linq;
using J4JSoftware.Logging;
using J4JSoftware.Utilities;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn.Sinks
{
    public abstract class RoslynDbSink<TSymbol> : SymbolSink<TSymbol>
        where TSymbol : class, ISymbol
    {
        protected readonly List<IAction<TSymbol>> _processors;

        protected RoslynDbSink(
            UniqueSymbols<TSymbol> uniqueSymbols,
            ActionsContext context,
            IJ4JLogger? logger,
            IEnumerable<IAction<TSymbol>>? processors = null
        )
            : base( context, logger )
        {
            Symbols = uniqueSymbols;

            _processors = processors?.ToList() ?? new List<IAction<TSymbol>>();

            if( !_processors.Any() )
                Logger?.Error( "No {0} processors defined for symbol {1}",
                    typeof(IAction<TSymbol>),
                    typeof(TSymbol) );
        }

        protected UniqueSymbols<TSymbol> Symbols { get; }

        public override bool InitializeSink( ISyntaxWalker syntaxWalker )
        {
            if( !base.InitializeSink( syntaxWalker ) )
                return false;

            Symbols.Clear();

            return true;
        }

        public override bool FinalizeSink( ISyntaxWalker syntaxWalker )
        {
            if( !base.FinalizeSink( syntaxWalker ) )
                return false;

            if( _processors == null )
            {
                Logger?.Error( "No processors defined for {0}", GetType() );
                return false;
            }

            var allOkay = true;

            foreach( var processor in _processors )
            {
                allOkay &= processor.Process( Symbols );

                if( !allOkay && Context.StopOnFirstError )
                    break;
            }

            return allOkay;
        }

        public override bool OutputSymbol( ISyntaxWalker syntaxWalker, TSymbol symbol )
        {
            if( !base.OutputSymbol( syntaxWalker, symbol ) )
                return false;

            Symbols.Add( symbol );

            return true;
        }
    }
}