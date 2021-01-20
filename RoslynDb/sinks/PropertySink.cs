using System.Collections.Generic;
using J4JSoftware.Logging;
using J4JSoftware.Utilities;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn.Sinks
{
    public class PropertySink : RoslynDbSink<IPropertySymbol>
    {
        public PropertySink(
            UniqueSymbols<IPropertySymbol> uniqueSymbols,
            ActionsContext context,
            IJ4JLogger logger,
            IEnumerable<IAction<IPropertySymbol>>? processors = null )
            : base( uniqueSymbols, context, logger, processors)
        {
        }
    }
}
