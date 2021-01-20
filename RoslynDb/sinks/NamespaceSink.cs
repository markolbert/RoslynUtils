using System.Collections.Generic;
using J4JSoftware.Logging;
using J4JSoftware.Utilities;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn.Sinks
{
    public class NamespaceSink : RoslynDbSink<INamespaceSymbol>
    {
        public NamespaceSink(
            UniqueSymbols<INamespaceSymbol> uniqueSymbols,
            ActionsContext context,
            IJ4JLogger logger,
            IEnumerable<IAction<INamespaceSymbol>>? processors = null )
            : base( uniqueSymbols, context, logger, processors )
        {
        }
    }
}
