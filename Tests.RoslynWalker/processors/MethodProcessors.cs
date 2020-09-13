using System;
using System.Collections.Generic;
using J4JSoftware.Logging;
using J4JSoftware.Roslyn;
using Microsoft.CodeAnalysis;

namespace Tests.RoslynWalker
{
    public sealed class MethodProcessors : RoslynDbProcessors<IMethodSymbol>
    {
        public MethodProcessors( 
            EntityFactories factories,
            Func<IJ4JLogger> loggerFactory 
        ) : base( factories, loggerFactory() )
        {
            var rootProcessor = new MethodProcessor( factories, loggerFactory() );

            Add( rootProcessor );
            Add( new ArgumentProcessor( factories, loggerFactory() ), rootProcessor );
        }

        protected override bool Initialize( IEnumerable<IMethodSymbol> symbols )
        {
            if( !base.Initialize( symbols ) )
                return false;

            EntityFactories.MarkSharpObjectUnsynchronized<MethodDb>();
            EntityFactories.MarkUnsynchronized<ArgumentDb>();
            EntityFactories.MarkSharpObjectUnsynchronized<MethodParametricTypeDb>(true);

            return true;
        }

        //protected override bool SetPredecessors()
        //{
        //    return SetPredecessor<ArgumentProcessor, MethodProcessor>();
        //}

        //// ensure the context object is able to reset itself so it can 
        //// handle multiple iterations
        //public bool Process( IEnumerable<IMethodSymbol> context )
        //{
        //    var allOkay = true;

        //    foreach( var processor in ExecutionSequence )
        //    {
        //        allOkay &= processor.Process( context );
        //    }

        //    return allOkay;
        //}
    }
}