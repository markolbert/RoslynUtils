using System.Collections.Generic;
using J4JSoftware.Logging;
using J4JSoftware.Utilities;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn.Sinks
{
    public class AttributeSink : RoslynDbSink<ISymbol>
    {
        public AttributeSink(
            UniqueSymbols<ISymbol> uniqueSymbols,
            ActionsContext context,
            IJ4JLogger logger,
            IEnumerable<IAction<ISymbol>>? processors = null )
            : base( uniqueSymbols, context, logger, processors)
        {
        }
    }
}
