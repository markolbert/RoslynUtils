using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn.Sinks
{
    public class AssemblySink : RoslynDbSink<IAssemblySymbol>
    {
        public AssemblySink(
            UniqueSymbols<IAssemblySymbol> uniqueSymbols,
            IJ4JLogger logger,
            ISymbolProcessors<IAssemblySymbol>? processors = null )
            : base( uniqueSymbols, logger, processors )
        {
        }
    }
}
