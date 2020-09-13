using System.Collections.Generic;
using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn.Sinks
{
    public class PropertySink : RoslynDbSink<IPropertySymbol>
    {
        public PropertySink(
            UniqueSymbols<IPropertySymbol> uniqueSymbols,
            IJ4JLogger logger,
            IProcessorCollection<IPropertySymbol>? processors = null )
            : base( uniqueSymbols, logger, processors)
        {
        }
    }
}
