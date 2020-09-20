using System.Collections.Generic;
using J4JSoftware.Logging;
using J4JSoftware.Roslyn;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn
{
    public abstract class RoslynDbProcessors<TSymbol> : AtomicTopologicalActions<TSymbol>
        where TSymbol : ISymbol
    {
        protected RoslynDbProcessors( 
            IRoslynDataLayer dataLayer,
            IJ4JLogger logger ) 
            : base( logger )
        {
            DataLayer = dataLayer;
        }

        protected IRoslynDataLayer DataLayer { get; }
    }
}