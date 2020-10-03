﻿using System.Collections.Generic;
using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn
{
    public class AtomicTopologicalActions<TSymbol> : TopologicallySortableCollection<IEnumerableProcessor<TSymbol>>, IProcessorCollection<TSymbol>
        where TSymbol : ISymbol
    {
        protected AtomicTopologicalActions( 
            ExecutionContext context,
            IJ4JLogger logger 
            )
        {
            Context = context;

            Logger = logger;
            Logger.SetLoggedType( this.GetType() );
        }

        protected IJ4JLogger Logger { get; }
        protected ExecutionContext Context { get; }

        // symbols must be able to reset so it can be iterated multiple times
        public virtual bool Process( IEnumerable<TSymbol> symbols )
        {
            if( !Initialize( symbols ) )
                return false;

            var allOkay = true;

            if( !Sort( out var processors, out var remainingEdges ) )
            {
                Logger.Error( "Couldn't topologically sort processors" );
                return false;
            }

            foreach( var processor in processors! )
            {
                allOkay &= processor.Process( symbols );

                if( !allOkay && Context.StopOnFirstError )
                    break;
            }

            return allOkay && Finalize( symbols );
        }

        protected virtual bool Initialize( IEnumerable<TSymbol> symbols ) => true;
        protected virtual bool Finalize( IEnumerable<TSymbol> symbols ) => true;
    }
}