using System;
using System.Collections.Generic;
using J4JSoftware.Logging;
using J4JSoftware.Utilities;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn
{
    public class TypeParametricTypeProcessor : BaseProcessorDb<List<ITypeSymbol>, ITypeParameterSymbol>
    {
        public TypeParametricTypeProcessor(
            IRoslynDataLayer dataLayer,
            ActionsContext context,
            IJ4JLogger? logger)
            : base("adding Parametric Types to the database", dataLayer, context, logger)
        {
        }

        protected override List<ITypeParameterSymbol> ExtractSymbols( List<ITypeSymbol> inputData )
        {
            var retVal = new List<ITypeParameterSymbol>();

            foreach (var symbol in inputData)
            {
                switch (symbol)
                {
                    // we handle ITypeParameterSymbols, which can either be the symbol itself
                    // or the ElementType of the symbol if it's an IArrayTypeSymbol

                    // also, here we >>only<< want ITypeParameterSymbols that are contained by
                    // a type -- the ones contained by IMethodSymbols are handled later, when
                    // methods are processed
                    case ITypeParameterSymbol tpSymbol:
                        if( tpSymbol.DeclaringType != null )
                            retVal.Add( tpSymbol );

                        break;

                    case IArrayTypeSymbol arraySymbol:
                        if( arraySymbol.ElementType is ITypeParameterSymbol { DeclaringType: { } } atpSymbol )
                            retVal.Add( atpSymbol );

                        break;
                }
            }

            return retVal;
        }

        // symbol is guaranteed to be an ITypeParameterSymbol with a non-null DeclaringType property
        protected override bool ProcessSymbol( ITypeParameterSymbol symbol ) =>
            DataLayer.GetParametricType( symbol, true, true ) != null;
    }
}
