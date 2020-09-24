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
            IJ4JLogger logger,
            IProcessorCollection<IAssemblySymbol>? processors = null )
            : base( uniqueSymbols, logger, processors )
        {
        }

        public override bool FinalizeSink(ISyntaxWalker syntaxWalker)
        {
            if (!base.FinalizeSink(syntaxWalker))
                return false;

            if (_processors == null)
            {
                Logger.Error<Type>("No processors defined for {0}", this.GetType());
                return false;
            }

            return _processors.Process(Symbols, StopOnFirstError);
        }

    }
}
