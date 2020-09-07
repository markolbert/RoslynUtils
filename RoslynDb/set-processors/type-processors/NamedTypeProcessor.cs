using System.Collections.Generic;
using System.Linq;
using System.Text;
using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn
{
    public class NamedTypeProcessor : BaseProcessorDb<ITypeSymbol, INamedTypeSymbol>
    {
        public NamedTypeProcessor(
            RoslynDbContext dbContext,
            IEntityFactories factories,
            ISymbolNamer symbolNamer,
            ISharpObjectTypeMapper sharpObjMapper,
            IJ4JLogger logger
        )
            : base( dbContext, factories, symbolNamer, sharpObjMapper, logger )
        {
        }

        protected override IEnumerable<INamedTypeSymbol> ExtractSymbols(object item)
        {
            if (!(item is ITypeSymbol typeSymbol))
            {
                Logger.Error("Supplied item is not an ITypeSymbol");
                yield break;
            }

            if (typeSymbol is IDynamicTypeSymbol || typeSymbol is IPointerTypeSymbol)
            {
                Logger.Error<string>("Unhandled ITypeSymbol '{0}'", typeSymbol.Name);
                yield break;
            }

            if (typeSymbol is IErrorTypeSymbol)
            {
                Logger.Error("ITypeSymbol is an IErrorTypeSymbol, ignored");
                yield break;
            }

            // we handle INamedTypeSymbols
            if (typeSymbol is INamedTypeSymbol ntSymbol)
                yield return ntSymbol;
        }

        protected override bool ProcessSymbol(INamedTypeSymbol symbol)
        {
            return symbol.IsGenericType && EntityFactories.Retrieve<GenericTypeDb>( symbol, out _, true )
                   || !symbol.IsGenericType && EntityFactories.Retrieve<FixedTypeDb>( symbol, out _, true );
        }
    }
}
