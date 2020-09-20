using System.Collections.Generic;
using System.Linq;
using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn
{
    public class TypeNamespaceProcessor : BaseProcessorDb<ITypeSymbol, INamespaceSymbol>
    {
        public TypeNamespaceProcessor(
            IRoslynDataLayer dataLayer,
            IJ4JLogger logger)
            : base(dataLayer, logger)
        {
        }

        protected override IEnumerable<INamespaceSymbol> ExtractSymbols( ISymbol item )
        {
            if( !( item is ITypeSymbol typeSymbol ) )
            {
                Logger.Error( "Supplied item is not an ITypeSymbol" );
                yield break;
            }

            if( typeSymbol.ContainingNamespace == null )
            {
                Logger.Information<string>( "ITypeSymbol '{0}' does not have a ContainingNamespace", typeSymbol.Name );
                yield break;
            }

            // ignore any namespaces already on file
            if( !DataLayer.SharpObjectInDatabase<NamespaceDb>( typeSymbol.ContainingNamespace ) )
                yield return typeSymbol.ContainingNamespace!;
        }

        protected override bool ProcessSymbol(INamespaceSymbol symbol)
        {
            var assemblyDb = DataLayer.GetAssembly( symbol.ContainingAssembly );

            if( assemblyDb == null )
                return false;

            var nsDb = DataLayer.GetNamespace( symbol, true );

            if (nsDb == null )
                return false;

            return DataLayer.GetAssemblyNamespace( assemblyDb, nsDb, true ) != null;
        }
    }
}
