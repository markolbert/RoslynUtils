using System.Collections.Generic;
using J4JSoftware.Logging;
using J4JSoftware.Utilities;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn
{
    public abstract class SimpleProcessorDb<TSymbol> : BaseProcessorDb<List<TSymbol>, TSymbol>
        where TSymbol: class, ISymbol
    {
        protected SimpleProcessorDb(
            string name,
            IRoslynDataLayer dataLayer,
            ActionsContext context,
            IJ4JLogger? logger
        )
            : base( name, dataLayer, context, logger )
        {
        }

        protected override List<TSymbol> ExtractSymbols( List<TSymbol> inputData ) => inputData;
    }
}