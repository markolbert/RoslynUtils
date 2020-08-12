using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using J4JSoftware.Logging;
using Serilog;

namespace J4JSoftware.Roslyn
{
    public abstract class AtomicProcessor<TInput> : IAtomicProcessor<TInput>
    {
        protected AtomicProcessor(
            IJ4JLogger logger
        )
        {
            Logger = logger;
            Logger.SetLoggedType( this.GetType() );
        }

        protected IJ4JLogger Logger { get; }

        public IAtomicProcessor<TInput> Predecessor { get; set; }

        public bool Process(TInput inputData)
        {
            if (!InitializeProcessor(inputData))
                return false;

            if (!ProcessInternal(inputData))
                return false;

            return FinalizeProcessor(inputData);
        }

        protected virtual bool InitializeProcessor(TInput inputData) => true;

        protected virtual bool FinalizeProcessor( TInput inputData ) => true;

        protected abstract bool ProcessInternal(TInput inputData);

        bool IAtomicProcessor.Process( object inputData )
        {
            if( inputData is TInput castData )
                return Process( castData );

            Logger.Error<Type, Type>( "Expected a {0} but got a {1}", typeof(TInput), inputData.GetType() );

            return false;
        }

        // processors are equal if they are the same type, so duplicate instances of the 
        // same type are always equal (and shouldn't be present in the processing set)
        public bool Equals( IAtomicProcessor<TInput>? other )
        {
            if (other == null)
                return false;

            return other.GetType() == GetType();
        }
    }
}
