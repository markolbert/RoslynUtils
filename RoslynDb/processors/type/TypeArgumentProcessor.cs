using System.Collections.Generic;
using System.Linq;
using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn
{
    public class TypeArgumentProcessor : BaseProcessorDb<ITypeSymbol, INamedTypeSymbol>
    {
        public TypeArgumentProcessor(
            IEntityFactories factories,
            IJ4JLogger logger
        )
            : base( factories, logger )
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
            if( !EntityFactories.Retrieve<ImplementableTypeDb>( symbol, out var declaringDb ) )
            {
                Logger.Error<string>( "Couldn't retrieve ImplementableTypeDb entity for '{0}'",
                    EntityFactories.GetFullName( symbol ) );

                return false;
            }

            var allOkay = true;

            for( var ordinal = 0; ordinal < symbol.TypeArguments.Length; ordinal++)
            {
                var typeArgSymbol = symbol.TypeArguments[ ordinal ];

                if( !EntityFactories.Retrieve<TypeDb>(typeArgSymbol, out var typeDb))
                {
                    Logger.Error<string, string>( "", 
                        EntityFactories.GetFullName( typeArgSymbol ),
                        EntityFactories.GetFullName( symbol ) );

                    allOkay = false;

                    continue;
                }

                var typeArgDb = EntityFactories.DbContext.TypeArguments
                    .FirstOrDefault( ta => ta.ArgumentTypeID == typeDb!.SharpObjectID && ta.Ordinal == ordinal );

                if( typeArgDb == null )
                {
                    typeArgDb = new TypeArgumentDb();

                    EntityFactories.DbContext.TypeArguments.Add( typeArgDb );
                }

                typeArgDb.DeclaringTypeID = declaringDb!.SharpObjectID;
                typeArgDb.ArgumentTypeID = typeDb!.SharpObjectID;
                typeArgDb.Ordinal = ordinal;
                typeArgDb.Synchronized = true;
            }

            return allOkay;
        }
    }
}
