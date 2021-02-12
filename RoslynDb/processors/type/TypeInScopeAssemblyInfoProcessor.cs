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
    public class TypeInScopeAssemblyInfoProcessor : BaseProcessorDb<List<ITypeSymbol>, IAssemblySymbol>
    {
        public TypeInScopeAssemblyInfoProcessor(
            IRoslynDataLayer dataLayer,
            WalkerContext context,
            IJ4JLogger? logger )
            : base( "updating Type InScopeAssemblyInfo in the database", dataLayer, context, logger )
        {
        }

        protected override List<IAssemblySymbol> ExtractSymbols( List<ITypeSymbol> inputData )
        {
            var retVal = new List<IAssemblySymbol>();

            foreach( var symbol in inputData )
            {
                var assemblySymbol = symbol is IArrayTypeSymbol arraySymbol
                    ? arraySymbol.ElementType.ContainingAssembly
                    : symbol.ContainingAssembly;

                if( assemblySymbol == null )
                {
                    Logger?.Information<string>( "ITypeSymbol '{0}' does not have a ContainingAssembly", symbol.Name );
                }
                else
                {
                    if( !( (WalkerContext) ExecutionContext ).InDocumentationScope( symbol.ContainingAssembly ) )
                        continue;

                    if( ( (WalkerContext) ExecutionContext ).HasCompiledProject( symbol.ContainingAssembly ) )
                        retVal.Add( symbol.ContainingAssembly );
                    else
                        Logger?.Error<string>( "Couldn't find CompiledProject for IAssemblySymbol '{0}'",
                            symbol.ContainingAssembly.ToUniqueName() );
                }
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