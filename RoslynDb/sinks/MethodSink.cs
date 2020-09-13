using System.Collections.Generic;
using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn.Sinks
{
    public class MethodSink : RoslynDbSink<IMethodSymbol>
    {
        public MethodSink(
            UniqueSymbols<IMethodSymbol> uniqueSymbols,
            IJ4JLogger logger,
            IProcessorCollection<IMethodSymbol>? processors = null )
            : base( uniqueSymbols, logger, processors )
        {
        }
    }
}
