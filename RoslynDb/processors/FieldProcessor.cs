using System.Collections.Generic;
using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn
{
    public class FieldProcessor : BaseProcessorDb<IFieldSymbol, IFieldSymbol>
    {
        public FieldProcessor( 
            IRoslynDataLayer dataLayer,
            IJ4JLogger logger ) 
            : base( dataLayer, logger )
        {
        }

        protected override IEnumerable<IFieldSymbol> ExtractSymbols( ISymbol item )
        {
            if (!(item is IFieldSymbol fieldSymbol) )
            {
                Logger.Error("Supplied item is not an IFieldSymbol");
                yield break;
            }

            yield return fieldSymbol;
        }

        protected override bool ProcessSymbol( IFieldSymbol symbol ) =>
            DataLayer.GetField( symbol, true ) != null;
    }
}
