using System.Collections.Generic;
using System.Linq;
using J4JSoftware.Logging;
using J4JSoftware.Utilities;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn
{
    public class NamespaceProcessor : BaseProcessorDb<INamespaceSymbol, INamespaceSymbol>
    {
        public NamespaceProcessor(
            IRoslynDataLayer dataLayer,
            ActionsContext context,
            IJ4JLogger logger)
            : base("adding Namespaces to the database", dataLayer, context, logger)
        {
        }

        protected override List<INamespaceSymbol> ExtractSymbols( IEnumerable<INamespaceSymbol> inputData )
        {
            return inputData.ToList();
        }

        protected override bool ProcessSymbol( INamespaceSymbol symbol )
        {
            var assemblyDb = DataLayer.GetAssembly(symbol.ContainingAssembly);

            if (assemblyDb == null)
                return false;

            var nsDb = DataLayer.GetNamespace(symbol, true, true);

            if (nsDb == null)
                return false;

            return DataLayer.GetAssemblyNamespace(assemblyDb, nsDb, true) != null;
        }
    }
}
