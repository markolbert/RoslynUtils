using System.Collections.Generic;
using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn
{
    public abstract class SymbolProcessors<TSymbol> : TopologicallySortedCollection<IAtomicProcessor<TSymbol>>, ISymbolProcessors<TSymbol> 
        where TSymbol : ISymbol
    {
        protected SymbolProcessors(
            IEnumerable<IAtomicProcessor<TSymbol>> items,
            IJ4JLogger logger
        )
            : base( items, logger )
        {
        }

        // ensure the context object is able to reset itself so it can 
        // handle multiple iterations
        public bool Process( IEnumerable<TSymbol> context, bool stopOnFirstError = false )
        {
            var allOkay = true;

            foreach( var processor in ExecutionSequence )
            {
                allOkay &= processor.Process( context, stopOnFirstError );

                if( !allOkay && stopOnFirstError )
                    break;
            }

            return allOkay;
        }
    }
}