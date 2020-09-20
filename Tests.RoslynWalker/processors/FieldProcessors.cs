using System;
using System.Collections.Generic;
using J4JSoftware.Logging;
using J4JSoftware.Roslyn;
using Microsoft.CodeAnalysis;

namespace Tests.RoslynWalker
{
    public sealed class FieldProcessors : RoslynDbProcessors<IFieldSymbol>
    {
        public FieldProcessors( 
            IRoslynDataLayer dataLayer,
            Func<IJ4JLogger> loggerFactory 
        ) : base( dataLayer, loggerFactory() )
        {
            Add( new FieldProcessor( dataLayer, loggerFactory() ) );
        }

        protected override bool Initialize( IEnumerable<IFieldSymbol> symbols )
        {
            if( !base.Initialize( symbols ) )
                return false;

            DataLayer.MarkSharpObjectUnsynchronized<FieldDb>();

            return true;
        }
    }
}