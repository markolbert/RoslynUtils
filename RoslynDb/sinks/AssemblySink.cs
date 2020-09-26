using System;
using System.Collections.Generic;
using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn.Sinks
{
    public class AssemblySink : RoslynDbSink<IAssemblySymbol>
    {
        public AssemblySink(
            UniqueSymbols<IAssemblySymbol> uniqueSymbols,
            ExecutionContext context,
            IJ4JLogger logger,
            IProcessorCollection<IAssemblySymbol>? processors = null )
            : base( uniqueSymbols, context, logger, processors )
        {
        }
    }
}
