﻿using System;
using System.Collections.Generic;
using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn
{
    public class TypeParametricTypeProcessor : BaseProcessorDb<ITypeSymbol, ITypeParameterSymbol>
    {
        public TypeParametricTypeProcessor(
            IRoslynDataLayer dataLayer,
            IJ4JLogger logger)
            : base(dataLayer, logger)
        {
        }

        protected override IEnumerable<ITypeParameterSymbol> ExtractSymbols( ISymbol item )
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

            // also, here we >>only<< want ITypeParameterSymbols that are contained by
            // a type -- the ones contained by IMethodSymbols are handled later, when
            // methods are processed
            if( typeSymbol is ITypeParameterSymbol tpSymbol && tpSymbol.DeclaringType != null )
                yield return tpSymbol;

            if( typeSymbol is IArrayTypeSymbol arraySymbol 
                && arraySymbol.ElementType is ITypeParameterSymbol atpSymbol
                && atpSymbol.DeclaringType != null )
                yield return atpSymbol;
        }

        // symbol is guaranteed to be an ITypeParameterSymbol with a non-null DeclaringType property
        protected override bool ProcessSymbol( ITypeParameterSymbol symbol ) =>
            DataLayer.GetParametricType( symbol, true ) != null;
    }
}
