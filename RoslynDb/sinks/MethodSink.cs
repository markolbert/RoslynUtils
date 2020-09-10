using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn.Sinks
{
    public class MethodSink : RoslynDbSink<IMethodSymbol>
    {
        public MethodSink(
            UniqueSymbols<IMethodSymbol> uniqueSymbols,
            IJ4JLogger logger,
            ISymbolProcessors<IMethodSymbol>? processors = null )
            : base( uniqueSymbols, logger, processors )
        {
        }
    }
}
