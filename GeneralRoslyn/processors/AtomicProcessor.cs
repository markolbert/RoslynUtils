using System;
using System.Collections.Generic;
using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn
{
    public abstract class AtomicProcessor<TSymbol> : IAtomicProcessor<TSymbol>
        where TSymbol : ISymbol
    {
        protected AtomicProcessor(
            IJ4JLogger logger
        )
        {
            Logger = logger;
            Logger.SetLoggedType( this.GetType() );
        }

        protected IJ4JLogger Logger { get; }

        public IAtomicProcessor<TSymbol>? Predecessor { get; set; }

        public bool Process( IEnumerable<TSymbol> inputData )
        {
            if( !InitializeProcessor( inputData ) )
                return false;

            if( !ProcessInternal( inputData ) )
                return false;

            return FinalizeProcessor( inputData );
        }

        protected virtual bool InitializeProcessor(IEnumerable<TSymbol> inputData) => true;

        protected virtual bool FinalizeProcessor(IEnumerable<TSymbol> inputData ) => true;

        protected abstract bool ProcessInternal(IEnumerable<TSymbol> inputData);

        bool IAtomicProcessor.Process( object inputData )
        {
            if( inputData is IEnumerable<TSymbol> castData )
                return Process( castData );

            Logger.Error<Type, Type>( "Expected a {0} but got a {1}", typeof(IEnumerable<TSymbol>), inputData.GetType() );

            return false;
        }

        // processors are equal if they are the same type, so duplicate instances of the 
        // same type are always equal (and shouldn't be present in the processing set)
        public bool Equals( IAtomicProcessor<TSymbol>? other )
        {
            if (other == null)
                return false;

            return other.GetType() == GetType();
        }
    }
}
