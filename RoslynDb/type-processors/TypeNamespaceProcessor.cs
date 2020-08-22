using System.Collections.Generic;
using System.Linq;
using System.Text;
using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn
{
    [RoslynProcessor(typeof(TypeAssemblyProcessor))]
    public class TypeNamespaceProcessor : BaseProcessorDb<INamespaceSymbol, List<ITypeSymbol>>
    {
        public TypeNamespaceProcessor(
            RoslynDbContext dbContext,
            ISymbolInfoFactory symbolInfo,
            IJ4JLogger logger
        )
            : base( dbContext, symbolInfo, logger )
        {
        }

        protected override bool ExtractSymbol( object item, out INamespaceSymbol? result )
        {
            result = null;

            if (!(item is ITypeSymbol typeSymbol))
            {
                Logger.Error("Supplied item is not an ITypeSymbol");
                return false;
            }

            result = typeSymbol.ContainingNamespace;

            return result != null;
        }

        protected override bool ProcessSymbol( INamespaceSymbol symbol )
        {
            var namespaces = GetDbSet<Namespace>();

            if( !GetByFullyQualifiedName<Namespace>( symbol, out var dbSymbol ) )
            {
                dbSymbol = new Namespace
                {
                    FullyQualifiedName = SymbolInfo.GetFullyQualifiedName( symbol ),
                    Name = SymbolInfo.GetName( symbol )
                };

                namespaces.Add( dbSymbol );
            }

            dbSymbol!.Synchronized = true;

            return true;
        }
    }
}
