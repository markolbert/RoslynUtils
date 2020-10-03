using System.Collections.Generic;
using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn.Sinks
{
    public class AttributeSink : RoslynDbSink<ISymbol>
    {
        public AttributeSink(
            UniqueSymbols<ISymbol> uniqueSymbols,
            ExecutionContext context,
            IJ4JLogger logger,
            IProcessorCollection<ISymbol>? processors = null )
            : base( uniqueSymbols, context, logger, processors)
        {
        }
    }
}
