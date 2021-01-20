using System.Collections.Generic;
using System.Linq;
using J4JSoftware.Logging;
using J4JSoftware.Utilities;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn
{
    public class TypeNamespaceProcessor : BaseProcessorDb<ITypeSymbol, INamespaceSymbol>
    {
        public TypeNamespaceProcessor(
            IRoslynDataLayer dataLayer,
            ActionsContext context,
            IJ4JLogger logger)
            : base("adding Type Namespaces to the database", dataLayer, context, logger)

        {
        }

        protected override List<INamespaceSymbol> ExtractSymbols( IEnumerable<ITypeSymbol> inputData )
        {
            var retVal = new List<INamespaceSymbol>();

            foreach (var symbol in inputData)
            {
                var nsSymbol = symbol is IArrayTypeSymbol arraySymbol
                    ? arraySymbol.ElementType.ContainingNamespace
                    : symbol.ContainingNamespace;

                if (nsSymbol == null)
                    Logger.Information<string>("ITypeSymbol '{0}' does not have a ContainingAssembly", symbol.Name);
                else retVal.Add(nsSymbol);
            }

            return retVal;
        }

        protected override bool ProcessSymbol(INamespaceSymbol symbol)
        {
            var assemblyDb = DataLayer.GetAssembly( symbol.ContainingAssembly );

            if( assemblyDb == null )
                return false;

            var nsDb = DataLayer.GetNamespace( symbol, true, true );

            if (nsDb == null )
                return false;

            return DataLayer.GetAssemblyNamespace( assemblyDb, nsDb, true ) != null;
        }
    }
}
