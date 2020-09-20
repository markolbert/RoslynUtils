using System.Collections.Generic;
using System.Linq;
using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn
{
    public class TypeArgumentProcessor : BaseProcessorDb<ITypeSymbol, INamedTypeSymbol>
    {
        public TypeArgumentProcessor(
            IRoslynDataLayer dataLayer,
            IJ4JLogger logger)
            : base(dataLayer, logger)
        {
        }

        protected override IEnumerable<INamedTypeSymbol> ExtractSymbols( ISymbol item )
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

            // we handle INamedTypeSymbols that have TypeArguments that aren't ITypeParameterSymbols
            if( typeSymbol is INamedTypeSymbol ntSymbol
                && ntSymbol.TypeArguments.Any( x => !( x is ITypeParameterSymbol ) ) )
                yield return ntSymbol;
        }

        protected override bool ProcessSymbol( INamedTypeSymbol symbol )
        {
            var declaringDb = DataLayer.GetGenericType( symbol );

            if( declaringDb == null )
            {
                Logger.Error<string>( "Couldn't retrieve ImplementableTypeDb entity for '{0}'",
                    symbol.ToFullName() );

                return false;
            }

            var allOkay = true;

            for( var ordinal = 0; ordinal < symbol.TypeArguments.Length; ordinal++)
            {
                var typeArgSymbol = symbol.TypeArguments[ ordinal ];

                if( DataLayer.GetTypeArgument(declaringDb, typeArgSymbol, ordinal, true) == null )
                {
                    Logger.Error<string>( "Couldn't find type for type argument '{0}' in database ",
                        typeArgSymbol.ToFullName() );

                    allOkay = false;

                    continue;
                }
            }

            return allOkay;
        }
    }
}
