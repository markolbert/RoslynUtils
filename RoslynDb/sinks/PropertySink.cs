using System.Collections.Generic;
using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn.Sinks
{
    public class PropertySink : RoslynDbSink<IPropertySymbol>
    {
        public PropertySink(
            UniqueSymbols<IPropertySymbol> uniqueSymbols,
            ExecutionContext context,
            IJ4JLogger logger,
            IProcessorCollection<IPropertySymbol>? processors = null )
            : base( uniqueSymbols, context, logger, processors)
        {
        }
    }
}
