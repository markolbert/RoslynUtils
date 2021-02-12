﻿#region license

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
    public class NamespaceProcessor : BaseProcessorDb<List<INamespaceSymbol>, INamespaceSymbol>
    {
        public NamespaceProcessor(
            IRoslynDataLayer dataLayer,
            ActionsContext context,
            IJ4JLogger? logger )
            : base( "adding Namespaces to the database", dataLayer, context, logger )
        {
        }

        protected override List<INamespaceSymbol> ExtractSymbols( List<INamespaceSymbol> inputData )
        {
            return inputData;
        }

        protected override bool ProcessSymbol( INamespaceSymbol symbol )
        {
            var assemblyDb = DataLayer.GetAssembly( symbol.ContainingAssembly );

            if( assemblyDb == null )
                return false;

            var nsDb = DataLayer.GetNamespace( symbol, true, true );

            if( nsDb == null )
                return false;

            return DataLayer.GetAssemblyNamespace( assemblyDb, nsDb, true ) != null;
        }
    }
}