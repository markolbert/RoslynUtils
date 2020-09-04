using System.Collections.Generic;
using System.Linq;
using System.Text;
using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn
{
    public class TypeAssemblyProcessor : AssemblyProcessorBase<ITypeSymbol>
    {
        public TypeAssemblyProcessor(
            RoslynDbContext dbContext,
            ISymbolNamer symbolNamer,
            IDocObjectTypeMapper docObjMapper,
            IJ4JLogger logger
        )
            : base( dbContext, symbolNamer, docObjMapper, logger )
        {
        }

        protected override IEnumerable<IAssemblySymbol> ExtractSymbols( object item )
        {
            if( !( item is ITypeSymbol typeSymbol ) )
            {
                Logger.Error("Supplied item is not an ITypeSymbol");
                yield break;
            }

            if( typeSymbol.ContainingAssembly == null )
            {
                Logger.Information<string>("ITypeSymbol '{0}' does not have a ContainingAssembly", typeSymbol.Name);
                yield break;
            }

            yield return typeSymbol.ContainingAssembly!;
        }
    }
}
