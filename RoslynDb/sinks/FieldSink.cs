using System.Collections.Generic;
using J4JSoftware.Logging;
using J4JSoftware.Utilities;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn.Sinks
{
    public class FieldSink : RoslynDbSink<IFieldSymbol>
    {
        public FieldSink(
            UniqueSymbols<IFieldSymbol> uniqueSymbols,
            ActionsContext context,
            IJ4JLogger logger,
            IEnumerable<IAction<IFieldSymbol>>? processors = null )
            : base( uniqueSymbols, context, logger, processors)
        {
        }
    }
}
