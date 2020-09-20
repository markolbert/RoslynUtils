using System.Collections.Generic;
using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn
{
    public class PropertyProcessor : BaseProcessorDb<IPropertySymbol, IPropertySymbol>
    {
        public PropertyProcessor(
            IRoslynDataLayer dataLayer,
            IJ4JLogger logger)
            : base(dataLayer, logger)
        {
        }

        protected override IEnumerable<IPropertySymbol> ExtractSymbols( ISymbol item )
        {
            if (!(item is IPropertySymbol propSymbol) )
            {
                Logger.Error("Supplied item is not an IPropertySymbol");
                yield break;
            }

            yield return propSymbol;
        }

        protected override bool ProcessSymbol( IPropertySymbol symbol ) =>
            DataLayer.GetProperty( symbol, true ) != null;
    }
}
