using System.Collections.Generic;
using J4JSoftware.Logging;
using J4JSoftware.Roslyn;
using J4JSoftware.Utilities;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn
{
    public abstract class RoslynDbProcessors<TSymbol> : Actions<TSymbol>
        where TSymbol : ISymbol
    {
        protected RoslynDbProcessors( 
            string procName,
            IRoslynDataLayer dataLayer,
            ActionsContext context,
            IJ4JLogger? logger ) 
            : base( context, logger )
        {
            Name = procName;
            DataLayer = dataLayer;
        }

        protected IRoslynDataLayer DataLayer { get; }

        public string Name { get; }

        protected override bool Initialize( IEnumerable<TSymbol> symbols )
        {
            Logger?.Information<string>( "Starting {0}...", Name );

            if( !base.Initialize( symbols ) )
                return false;

            return true;
        }

        protected override bool Finalize( IEnumerable<TSymbol> symbols )
        {
            Logger?.Information<string>("... finished {0}", Name);

            return base.Finalize( symbols );
        }
    }
}