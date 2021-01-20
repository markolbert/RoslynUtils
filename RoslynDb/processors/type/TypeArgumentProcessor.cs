using System.Collections.Generic;
using System.Linq;
using J4JSoftware.Logging;
using J4JSoftware.Utilities;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn
{
    public class TypeArgumentProcessor : BaseProcessorDb<ITypeSymbol, INamedTypeSymbol>
    {
        public TypeArgumentProcessor(
            IRoslynDataLayer dataLayer,
            ActionsContext context,
            IJ4JLogger logger)
            : base("adding Type Arguments to the database", dataLayer, context, logger)
        {
        }

        protected override List<INamedTypeSymbol> ExtractSymbols( IEnumerable<ITypeSymbol> inputData )
        {
            var retVal = new List<INamedTypeSymbol>();

            foreach (var symbol in inputData)
            {
                if( symbol is INamedTypeSymbol ntSymbol
                    && ntSymbol.TypeArguments.Any( x => !( x is ITypeParameterSymbol ) ) )
                    retVal.Add( ntSymbol );
            }

            return retVal;
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
