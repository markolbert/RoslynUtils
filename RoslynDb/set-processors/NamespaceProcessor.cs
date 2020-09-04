using System.Collections.Generic;
using System.Linq;
using System.Text;
using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn
{
    public class NamespaceProcessor : NamespaceProcessorBase<INamespaceSymbol>
    {
        public NamespaceProcessor(
            RoslynDbContext dbContext,
            ISymbolNamer symbolNamer,
            IDocObjectTypeMapper docObjMapper,
            IJ4JLogger logger
        )
            : base( dbContext, symbolNamer, docObjMapper, logger )
        {
        }

        protected override IEnumerable<INamespaceSymbol> ExtractSymbols( object item )
        {
            if (!(item is INamespaceSymbol nsSymbol ))
            {
                Logger.Error("Supplied item is not an INamespaceSymbol");
                yield break;
            }

            yield return nsSymbol!;
        }
    }
}
