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
using System.Linq;
using J4JSoftware.Logging;
using J4JSoftware.Utilities;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn
{
    public class TypeArgumentProcessor : BaseProcessorDb<List<ITypeSymbol>, INamedTypeSymbol>
    {
        public TypeArgumentProcessor(
            IRoslynDataLayer dataLayer,
            ActionsContext context,
            IJ4JLogger? logger )
            : base( "adding Type Arguments to the database", dataLayer, context, logger )
        {
        }

        protected override List<INamedTypeSymbol> ExtractSymbols( List<ITypeSymbol> inputData )
        {
            var retVal = new List<INamedTypeSymbol>();

            foreach( var symbol in inputData )
                if( symbol is INamedTypeSymbol ntSymbol
                    && ntSymbol.TypeArguments.Any( x => !( x is ITypeParameterSymbol ) ) )
                    retVal.Add( ntSymbol );

            return retVal;
        }

        protected override bool ProcessSymbol( INamedTypeSymbol symbol )
        {
            var declaringDb = DataLayer.GetGenericType( symbol );

            if( declaringDb == null )
            {
                Logger?.Error<string>( "Couldn't retrieve ImplementableTypeDb entity for '{0}'",
                    symbol.ToFullName() );

                return false;
            }

            var allOkay = true;

            for( var ordinal = 0; ordinal < symbol.TypeArguments.Length; ordinal++ )
            {
                var typeArgSymbol = symbol.TypeArguments[ ordinal ];

                if( DataLayer.GetTypeArgument( declaringDb, typeArgSymbol, ordinal, true ) != null )
                    continue;

                Logger?.Error<string>( "Couldn't find type for type argument '{0}' in database ",
                    typeArgSymbol.ToFullName() );

                allOkay = false;
            }

            return allOkay;
        }
    }
}