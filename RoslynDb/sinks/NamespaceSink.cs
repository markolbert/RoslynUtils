using System.Collections.Generic;
using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn.Sinks
{
    public class NamespaceSink : RoslynDbSink<INamespaceSymbol>
    {
        public NamespaceSink(
            UniqueSymbols<INamespaceSymbol> uniqueSymbols,
            ExecutionContext context,
            IJ4JLogger logger,
            IProcessorCollection<INamespaceSymbol>? processors = null )
            : base( uniqueSymbols, context, logger, processors )
        {
        }
    }
}
