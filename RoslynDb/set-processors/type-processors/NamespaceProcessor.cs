using System.Collections.Generic;
using System.Linq;
using System.Text;
using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn
{
    public class NamespaceProcessor : BaseProcessorDb<ITypeSymbol, INamespaceSymbol>
    {
        public NamespaceProcessor(
            RoslynDbContext dbContext,
            ISymbolNamer symbolNamer,
            IJ4JLogger logger
        )
            : base( dbContext, symbolNamer, logger )
        {
        }

        protected override IEnumerable<INamespaceSymbol> ExtractSymbols( object item )
        {
            if (!(item is ITypeSymbol typeSymbol))
            {
                Logger.Error("Supplied item is not an ITypeSymbol");
                yield break;
            }

            if( typeSymbol.ContainingNamespace == null )
            {
                Logger.Information<string>( "ITypeSymbol '{0}' does not have a ContainingNamespace", typeSymbol.Name );
                yield break;
            }

            yield return typeSymbol.ContainingNamespace!;
        }

        protected override bool ProcessSymbol( INamespaceSymbol symbol )
        {
            var namespaces = GetDbSet<NamespaceDb>();

            if( !GetByFullyQualifiedName<NamespaceDb>( symbol, out var dbSymbol ) )
            {
                dbSymbol = new NamespaceDb
                {
                    FullyQualifiedName = SymbolNamer.GetFullyQualifiedName( symbol ),
                    Name = SymbolNamer.GetName( symbol )
                };

                namespaces.Add( dbSymbol );
            }

            dbSymbol!.Synchronized = true;

            return true;
        }
    }
}
