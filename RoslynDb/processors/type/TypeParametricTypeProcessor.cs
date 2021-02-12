#region license

// Copyright 2021 Mark A. Olbert
// 
// This library or program 'RoslynDb' is free software: you can redistribute it
// and/or modify it under the terms of the GNU General Public License as
// published by the Free Software Foundation, either version 3 of the License,
// or (at your option) any later version.
// 
// This library or program is distributed in the hope that it will be useful, but
// WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// General Public License for more details.
// 
// You should have received a copy of the GNU General Public License along with
// this library or program.  If not, see <https://www.gnu.org/licenses/>.

#endregion

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
            IJ4JLogger? logger )
            : base( "adding Parametric Types to the database", dataLayer, context, logger )
        {
        }

        protected override List<ITypeParameterSymbol> ExtractSymbols( List<ITypeSymbol> inputData )
        {
            var retVal = new List<ITypeParameterSymbol>();

            foreach( var symbol in inputData )
                switch( symbol )
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

            return retVal;
        }

        // symbol is guaranteed to be an ITypeParameterSymbol with a non-null DeclaringType property
        protected override bool ProcessSymbol( ITypeParameterSymbol symbol )
        {
            return DataLayer.GetParametricType( symbol, true, true ) != null;
        }
    }
}