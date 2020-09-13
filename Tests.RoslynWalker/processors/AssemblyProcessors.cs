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
            EntityFactories factories,
            Func<IJ4JLogger> loggerFactory 
        ) : base( factories, loggerFactory() )
        {
            Add( new AssemblyProcessor( factories, loggerFactory() ) );
        }

        protected override bool Initialize( IEnumerable<IAssemblySymbol> symbols )
        {
            if( !base.Initialize( symbols ) )
                return false;

            EntityFactories.MarkSharpObjectUnsynchronized<AssemblyDb>(true);

            return true;
        }
    }
}