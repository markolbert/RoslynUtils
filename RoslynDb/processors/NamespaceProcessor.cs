using System.Collections.Generic;
using System.Linq;
using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn
{
    public class NamespaceProcessor : BaseProcessorDb<INamespaceSymbol, INamespaceSymbol>
    {
        public NamespaceProcessor(
            IRoslynDataLayer dataLayer,
            IJ4JLogger logger)
            : base(dataLayer, logger)
        {
        }

        protected override IEnumerable<INamespaceSymbol> ExtractSymbols( ISymbol item )
        {
            if( !( item is INamespaceSymbol nsSymbol ) )
            {
                Logger.Error( "Supplied item is not an INamespaceSymbol" );
                yield break;
            }

            yield return nsSymbol!;
        }

        protected override bool ProcessSymbol( INamespaceSymbol symbol )
        {
            var assemblyDb = DataLayer.GetAssembly(symbol.ContainingAssembly);

            if (assemblyDb == null)
                return false;

            var nsDb = DataLayer.GetNamespace(symbol, true);

            if (nsDb == null)
                return false;

            return DataLayer.GetAssemblyNamespace(assemblyDb, nsDb, true) != null;
        }
    }
}
