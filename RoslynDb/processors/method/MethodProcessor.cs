using System.Collections.Generic;
using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn
{
    public class MethodProcessor : BaseProcessorDb<IMethodSymbol, IMethodSymbol>
    {
        public MethodProcessor(
            IRoslynDataLayer dataLayer,
            IJ4JLogger logger)
            : base(dataLayer, logger)
        {
        }

        protected override IEnumerable<IMethodSymbol> ExtractSymbols( ISymbol item )
        {
            if (!(item is IMethodSymbol methodSymbol) )
            {
                Logger.Error("Supplied item is not an IMethodSymbol");
                yield break;
            }

            yield return methodSymbol;
        }

        protected override bool ProcessSymbol( IMethodSymbol symbol ) =>
            DataLayer.GetMethod( symbol, true ) != null;
    }
}
