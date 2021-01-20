using System;
using System.Collections.Generic;
using J4JSoftware.Logging;
using J4JSoftware.Utilities;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn.Sinks
{
    public class AssemblySink : RoslynDbSink<IAssemblySymbol>
    {
        public AssemblySink(
            UniqueSymbols<IAssemblySymbol> uniqueSymbols,
            ActionsContext context,
            IJ4JLogger logger,
            IEnumerable<IAction<IAssemblySymbol>>? processors = null )
            : base( uniqueSymbols, context, logger, processors )
        {
        }
    }
}
