using System;
using System.Collections.Generic;
using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn
{
    public abstract class EnumerableProcessorBase<TItem> : IEnumerableProcessor<TItem>
    {
        protected EnumerableProcessorBase(
            IJ4JLogger logger
        )
        {
            Logger = logger;
            Logger.SetLoggedType( this.GetType() );
        }

        protected IJ4JLogger Logger { get; }

        public bool Process( IEnumerable<TItem> inputData )
        {
            if( !PreLoopInitialization( inputData ) )
                return false;

            if( !ProcessLoop( inputData ) )
                return false;

            return PostLoopFinalization( inputData );
        }

        protected virtual bool PreLoopInitialization(IEnumerable<TItem> inputData) => true;

        protected virtual bool PostLoopFinalization(IEnumerable<TItem> inputData ) => true;

        protected abstract bool ProcessLoop( IEnumerable<TItem> inputData );

        // processors are equal if they are the same type, so duplicate instances of the 
        // same type are always equal (and shouldn't be present in the processing set)
        public bool Equals( IEnumerableProcessor<TItem>? other )
        {
            if (other == null)
                return false;

            return other.GetType() == GetType();
        }
    }
}
