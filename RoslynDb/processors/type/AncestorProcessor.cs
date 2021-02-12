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
    public class AncestorProcessor : BaseProcessorDb<List<ITypeSymbol>, ITypeSymbol>
    {
        public AncestorProcessor(
            IRoslynDataLayer dataLayer,
            ActionsContext context,
            IJ4JLogger? logger )
            : base( "adding Type Ancestors to the database", dataLayer, context, logger )
        {
        }

        protected override List<ITypeSymbol> ExtractSymbols( List<ITypeSymbol> inputData )
        {
            var retVal = new List<ITypeSymbol>();

            foreach( var symbol in inputData )
                switch( symbol )
                {
                    case IDynamicTypeSymbol dtSymbol:
                        Logger?.Error<string>( "IDynamicTypeSymbols are not supported ('{0}')", symbol.Name );
                        break;

                    case IPointerTypeSymbol ptSymbol:
                        Logger?.Error<string>( "IPointerTypeSymbols are not supported ('{0}')", symbol.Name );
                        break;

                    case IErrorTypeSymbol errSymbol:
                        Logger?.Error<string>( "IErrorTypeSymbols are not supported ('{0}')", symbol.Name );
                        break;

                    default:
                        retVal.Add( symbol );
                        break;
                }

            return retVal;
        }

        protected override bool ProcessSymbol( ITypeSymbol typeSymbol )
        {
            var typeDb = DataLayer.GetUnspecifiedType( typeSymbol );

            if( typeDb == null )
                return false;

            // if typeSymbol is System.Object, which has no base type, we're done
            if( typeSymbol.BaseType == null )
                return true;

            if( !ProcessAncestor( typeDb!, typeSymbol.BaseType ) )
                return false;

            var allOkay = true;

            foreach( var interfaceSymbol in typeSymbol.Interfaces )
                allOkay &= ProcessAncestor( typeDb!, interfaceSymbol );

            return allOkay;
        }

        private bool ProcessAncestor( BaseTypeDb typeDb, INamedTypeSymbol ancestorSymbol )
        {
            var ancestorDb = DataLayer.GetImplementableType( ancestorSymbol );

            if( ancestorDb == null )
                return false;

            var typeAncestorDb = DataLayer.GetTypeAncestor( typeDb, ancestorDb!, true );

            if( typeAncestorDb == null )
                return false;

            typeAncestorDb.Synchronized = true;

            return true;
        }
    }
}