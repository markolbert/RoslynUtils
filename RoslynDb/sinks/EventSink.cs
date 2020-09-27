using System.Collections.Generic;
using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn.Sinks
{
    public class EventSink : RoslynDbSink<IEventSymbol>
    {
        public EventSink(
            UniqueSymbols<IEventSymbol> uniqueSymbols,
            ExecutionContext context,
            IJ4JLogger logger,
            IProcessorCollection<IEventSymbol>? processors = null )
            : base( uniqueSymbols, context, logger, processors)
        {
        }
    }
}
