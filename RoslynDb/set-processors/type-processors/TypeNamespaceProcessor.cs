using System.Collections.Generic;
using System.Linq;
using System.Text;
using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn
{
    public class TypeNamespaceProcessor : BaseProcessorDb<ITypeSymbol, INamespaceSymbol>
    {
        public TypeNamespaceProcessor(
            RoslynDbContext dbContext,
            IEntityFactories factories,
            ISymbolNamer symbolNamer,
            ISharpObjectTypeMapper sharpObjMapper,
            IJ4JLogger logger
        )
            : base( dbContext, factories, symbolNamer, sharpObjMapper, logger )
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

        protected override bool ProcessSymbol(INamespaceSymbol symbol) =>
            EntityFactories.Retrieve<NamespaceDb>(symbol, out _, true);
    }
}
