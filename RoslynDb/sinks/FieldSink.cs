using System.Collections.Generic;
using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn.Sinks
{
    public class FieldSink : RoslynDbSink<IFieldSymbol>
    {
        public FieldSink(
            UniqueSymbols<IFieldSymbol> uniqueSymbols,
            IJ4JLogger logger,
            IProcessorCollection<IFieldSymbol>? processors = null )
            : base( uniqueSymbols, logger, processors)
        {
        }
    }
}
