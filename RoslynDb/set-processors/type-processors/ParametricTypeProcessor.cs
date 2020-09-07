using System.Collections.Generic;
using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn
{
    public class ParametricTypeProcessor : BaseProcessorDb<ITypeSymbol, ITypeParameterSymbol>
    {
        public ParametricTypeProcessor(
            RoslynDbContext dbContext,
            IEntityFactories factories,
            ISymbolNamer symbolNamer,
            ISharpObjectTypeMapper sharpObjMapper,
            IJ4JLogger logger
        )
            : base( dbContext, factories, symbolNamer, sharpObjMapper, logger )
        {
        }

        protected override IEnumerable<ITypeParameterSymbol> ExtractSymbols( object item )
        {
            if( !( item is ITypeSymbol typeSymbol ) )
            {
                Logger.Error( "Supplied item is not an ITypeSymbol" );
                yield break;
            }

            if( typeSymbol is IDynamicTypeSymbol || typeSymbol is IPointerTypeSymbol )
            {
                Logger.Error<string>( "Unhandled ITypeSymbol '{0}'", typeSymbol.Name );
                yield break;
            }

            if( typeSymbol is IErrorTypeSymbol )
            {
                Logger.Error( "ITypeSymbol is an IErrorTypeSymbol, ignored" );
                yield break;
            }

            // we handle ITypeParameterSymbols, which can either be the symbol itself
            // or the ElementType of the symbol if it's an IArrayTypeSymbol
            if( typeSymbol is ITypeParameterSymbol tpSymbol )
                yield return tpSymbol;

            if( typeSymbol is IArrayTypeSymbol arraySymbol 
                && arraySymbol.ElementType is ITypeParameterSymbol atpSymbol )
                yield return atpSymbol;
        }

        // symbol is guaranteed to be an ITypeParameterSymbol 
        protected override bool ProcessSymbol( ITypeParameterSymbol symbol )
        {
            if (!RetrieveAssembly(symbol.ContainingAssembly, out var assemblyDb))
                return false;

            if (!RetrieveNamespace(symbol.ContainingNamespace, out var nsDb))
                return false;

            if( !EntityFactories.Retrieve<ParametricTypeDb>( symbol, out var dbSymbol, true ) )
            {
                Logger.Error<string>("Could not retrieve ParametricTypeDb entity for '{0}'",
                    EntityFactories.GetFullyQualifiedName(symbol));

                return false;
            }

            MarkSynchronized( dbSymbol! );

            dbSymbol!.AssemblyID = assemblyDb!.SharpObjectID;
            dbSymbol.NamespaceID = nsDb!.SharpObjectID;

            return true;
        }
    }
}
