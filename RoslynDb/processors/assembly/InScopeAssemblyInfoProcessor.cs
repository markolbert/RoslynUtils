using System;
using System.Collections.Generic;
using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn
{
    public class InScopeAssemblyInfoProcessor : BaseProcessorDb<IAssemblySymbol, IAssemblySymbol>
    {
        public InScopeAssemblyInfoProcessor(
            IRoslynDataLayer dataLayer,
            ExecutionContext context,
            IJ4JLogger logger)
            : base("updating InScopeAssembly information in the database", dataLayer, context, logger)
        {
        }

        protected override List<IAssemblySymbol> ExtractSymbols( IEnumerable<IAssemblySymbol> inputData )
        {
            var retVal = new List<IAssemblySymbol>();

            foreach( var symbol in inputData )
            {
                if( !ExecutionContext.InDocumentationScope( symbol ) ) 
                    continue;
                
                if( ExecutionContext.HasCompiledProject( symbol ) )
                    retVal.Add( symbol );
                else
                    Logger.Error<string>( "Couldn't find CompiledProject for IAssemblySymbol '{0}'",
                        symbol.ToUniqueName() );
            }

            return retVal;
        }

        // symbol is guaranteed to be in the documentation scope and having an associated CompiledProject
        protected override bool ProcessSymbol( IAssemblySymbol symbol ) =>
            DataLayer.GetInScopeAssemblyInfo( ExecutionContext[ symbol ]!, true, true ) != null;
    }
}
