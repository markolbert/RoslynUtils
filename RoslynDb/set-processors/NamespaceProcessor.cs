using System.Collections.Generic;
using System.Linq;
using System.Text;
using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn
{
    public class NamespaceProcessor : BaseProcessorDb<INamespaceSymbol, INamespaceSymbol>
    {
        public NamespaceProcessor(
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
            if( !( item is INamespaceSymbol nsSymbol ) )
            {
                Logger.Error( "Supplied item is not an INamespaceSymbol" );
                yield break;
            }

            yield return nsSymbol!;
        }

        protected override bool ProcessSymbol( INamespaceSymbol symbol ) =>
            EntityFactories.Retrieve<NamespaceDb>( symbol, out _, true );
    }
}
