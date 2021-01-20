using System;
using System.Collections.Generic;
using J4JSoftware.Logging;
using J4JSoftware.Roslyn;
using J4JSoftware.Utilities;
using Microsoft.CodeAnalysis;

namespace Tests.RoslynWalker
{
    public sealed class AssemblyProcessors : RoslynDbProcessors<IAssemblySymbol>
    {
        public AssemblyProcessors( 
            IRoslynDataLayer dataLayer,
            WalkerContext context,
            Func<IJ4JLogger> loggerFactory 
        ) : base( "Assembly processing", dataLayer,  context, loggerFactory() )
        {
            var node = AddIndependentNode( new AssemblyProcessor( dataLayer, context, loggerFactory() ) );

            AddDependentNode( new InScopeAssemblyInfoProcessor( dataLayer, context, loggerFactory() ), node.Value );
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