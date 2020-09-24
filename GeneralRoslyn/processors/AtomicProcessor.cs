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

        public ISyntaxWalker? SyntaxWalker { get; private set; } = null;
        public bool StopOnFirstError { get; private set; } = false;

        public virtual bool Initialize( IAtomicActionConfiguration config )
        {
            StopOnFirstError = config.StopOnFirstError;
            SyntaxWalker = config.SyntaxWalker;

            return true;
        }

        public bool Process( IEnumerable<TSymbol> inputData )
        {
            //if( !InitializeProcessor( inputData ) )
            //    return false;

            if( !ProcessInternal( inputData ) )
                return false;

            return FinalizeProcessor( inputData );
        }

        //protected virtual bool InitializeProcessor(IEnumerable<TSymbol> inputData) => true;

        protected virtual bool FinalizeProcessor(IEnumerable<TSymbol> inputData ) => true;

        protected abstract bool ProcessInternal( IEnumerable<TSymbol> inputData );

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
