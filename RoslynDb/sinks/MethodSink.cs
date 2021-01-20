using System.Collections.Generic;
using J4JSoftware.Logging;
using J4JSoftware.Utilities;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn.Sinks
{
    public class MethodSink : RoslynDbSink<IMethodSymbol>
    {
        public MethodSink(
            UniqueSymbols<IMethodSymbol> uniqueSymbols,
            ActionsContext context,
            IJ4JLogger logger,
            IEnumerable<IAction<IMethodSymbol>>? processors = null )
            : base( uniqueSymbols,context, logger, processors )
        {
        }
    }
}
