using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using J4JSoftware.Logging;
using Serilog;

namespace J4JSoftware.Roslyn
{
    public abstract class AtomicProcessor<TSource> : IAtomicProcessor<TSource>
    {
        protected AtomicProcessor(
            IJ4JLogger logger
        )
        {
            Logger = logger;
            Logger.SetLoggedType( this.GetType() );
        }

        protected IJ4JLogger Logger { get; }

        public IAtomicProcessor<TSource> Predecessor { get; set; }

        public bool Process(TSource inputData)
        {
            if (!InitializeProcessor(inputData))
                return false;

            if (!ProcessInternal(inputData))
                return false;

            return FinalizeProcessor(inputData);
        }

        protected virtual bool InitializeProcessor(TSource inputData) => true;

        protected virtual bool FinalizeProcessor( TSource inputData ) => true;

        protected abstract bool ProcessInternal(TSource inputData);

        bool IAtomicProcessor.Process( object inputData )
        {
            if( inputData is TSource castData )
                return Process( castData );

            Logger.Error<Type, Type>( "Expected a {0} but got a {1}", typeof(TSource), inputData.GetType() );

            return false;
        }

        // processors are equal if they are the same type, so duplicate instances of the 
        // same type are always equal (and shouldn't be present in the processing set)
        public bool Equals( IAtomicProcessor<TSource>? other )
        {
            if (other == null)
                return false;

            return other.GetType() == GetType();
        }
    }
}
