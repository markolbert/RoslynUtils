using System.Collections.Generic;
using J4JSoftware.Logging;
using J4JSoftware.Utilities;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn.Sinks
{
    public class EventSink : RoslynDbSink<IEventSymbol>
    {
        public EventSink(
            UniqueSymbols<IEventSymbol> uniqueSymbols,
            ActionsContext context,
            IJ4JLogger logger,
            IEnumerable<IAction<IEventSymbol>>? processors = null )
            : base( uniqueSymbols, context, logger, processors)
        {
        }
    }
}
