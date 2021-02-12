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
using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn
{
    public class InScopeAssemblyInfoProcessor : BaseProcessorDb<List<IAssemblySymbol>, IAssemblySymbol>
    {
        public InScopeAssemblyInfoProcessor(
            IRoslynDataLayer dataLayer,
            WalkerContext context,
            IJ4JLogger? logger )
            : base( "updating InScopeAssembly information in the database", dataLayer, context, logger )
        {
        }

        protected override List<IAssemblySymbol> ExtractSymbols( List<IAssemblySymbol> inputData )
        {
            var retVal = new List<IAssemblySymbol>();

            foreach( var symbol in inputData )
            {
                if( !( (WalkerContext) ExecutionContext ).InDocumentationScope( symbol ) )
                    continue;

                if( ( (WalkerContext) ExecutionContext ).HasCompiledProject( symbol ) )
                    retVal.Add( symbol );
                else
                    Logger?.Error<string>( "Couldn't find CompiledProject for IAssemblySymbol '{0}'",
                        symbol.ToUniqueName() );
            }

            return retVal;
        }

        // symbol is guaranteed to be in the documentation scope and having an associated CompiledProject
        protected override bool ProcessSymbol( IAssemblySymbol symbol )
        {
            return DataLayer.GetInScopeAssemblyInfo( ( (WalkerContext) ExecutionContext )[ symbol ]!, true, true ) !=
                   null;
        }
    }
}