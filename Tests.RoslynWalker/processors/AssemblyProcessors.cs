using System;
using System.Collections.Generic;
using J4JSoftware.Logging;
using J4JSoftware.Roslyn;
using Microsoft.CodeAnalysis;

namespace Tests.RoslynWalker
{
    public sealed class AssemblyProcessors : RoslynDbProcessors<IAssemblySymbol>
    {
        public AssemblyProcessors( 
            IRoslynDataLayer dataLayer,
            ExecutionContext context,
            Func<IJ4JLogger> loggerFactory 
        ) : base( dataLayer,  context, loggerFactory() )
        {
            var node = Add( new AssemblyProcessor( dataLayer, context, loggerFactory() ) );

            Add( new InScopeAssemblyInfoProcessor( dataLayer, context, loggerFactory() ), node );
        }

        protected override bool Initialize( IEnumerable<IAssemblySymbol> symbols )
        {
            if( !base.Initialize( symbols ) )
                return false;

            DataLayer.MarkSharpObjectUnsynchronized<AssemblyDb>();

            return true;
        }
    }
}