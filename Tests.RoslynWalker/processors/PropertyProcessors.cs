using System;
using System.Collections.Generic;
using J4JSoftware.Logging;
using J4JSoftware.Roslyn;
using Microsoft.CodeAnalysis;

namespace Tests.RoslynWalker
{
    public sealed class PropertyProcessors : RoslynDbProcessors<IPropertySymbol>
    {
        public PropertyProcessors( 
            EntityFactories factories,
            Func<IJ4JLogger> loggerFactory 
        ) : base( factories, loggerFactory() )
        {
            var rootProcessor = new PropertyProcessor(factories, loggerFactory());

            Add( rootProcessor );
            Add( new ParameterProcessor( factories, loggerFactory() ), rootProcessor );
        }

        protected override bool Initialize( IEnumerable<IPropertySymbol> symbols )
        {
            if( !base.Initialize( symbols ) )
                return false;

            EntityFactories.MarkSharpObjectUnsynchronized<PropertyDb>();
            EntityFactories.MarkUnsynchronized<PropertyParameterDb>(true);

            return true;
        }

        //protected override bool SetPredecessors()
        //{
        //    return SetPredecessor<ParameterProcessor, PropertyProcessor>();
        //}
    }
}