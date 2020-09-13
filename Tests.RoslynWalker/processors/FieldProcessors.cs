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
            EntityFactories factories,
            Func<IJ4JLogger> loggerFactory 
        ) : base( factories, loggerFactory() )
        {
            Add( new FieldProcessor( factories, loggerFactory() ) );
        }

        protected override bool Initialize( IEnumerable<IFieldSymbol> symbols )
        {
            if( !base.Initialize( symbols ) )
                return false;

            EntityFactories.MarkSharpObjectUnsynchronized<FieldDb>();

            return true;
        }

        //// fields only have a single processor
        //protected override bool SetPredecessors() => true;
    }
}