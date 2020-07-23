using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn.Sinks
{
    public abstract class RoslynDbSink<TSymbol> : SymbolSink<TSymbol>
        where TSymbol : class, ISymbol
    {
        protected RoslynDbSink(
            RoslynDbContext dbContext,
            SymbolNamers symbolNamers,
            IJ4JLogger logger
        )
            : base( logger )
        {
            DbContext = dbContext;
            SymbolNamers = symbolNamers;
        }

        protected RoslynDbContext DbContext { get; }
        protected SymbolNamers SymbolNamers { get; }
    }
}