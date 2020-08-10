using System;
using J4JSoftware.Logging;
using Microsoft.EntityFrameworkCore;

namespace J4JSoftware.Roslyn
{
    public abstract class BaseProcessor<TEntity, TSource> : IRoslynProcessor<TSource>, ITopologicalSort<BaseProcessor<TEntity, TSource>>
    {
        private BaseProcessor<TEntity, TSource>? _predecessor;

        protected BaseProcessor(
            RoslynDbContext dbContext,
            IJ4JLogger logger
        )
        {
            DbContext = dbContext;

            Logger = logger;
            Logger.SetLoggedType( this.GetType() );
        }

        protected RoslynDbContext DbContext { get; }
        protected IJ4JLogger Logger { get; }

        public Type SupportedType => typeof(TEntity);

        public object? Predecessor
        {
            get => _predecessor;

            set
            {
                if( value is BaseProcessor<TEntity, TSource> castValue )
                {
                    _predecessor = castValue;
                    return;
                }

                throw new InvalidCastException(
                    $"{nameof( Predecessor )} expects a {typeof( BaseProcessor<TEntity, TSource> )}, was assigned a {value.GetType()} instead" );
            }
        }

        public bool Process( TSource inputData )
        {
            if( !InitializeProcessor( inputData ) )
                return false;

            if( !ProcessInternal( inputData ) )
                return false;

            return FinalizeProcessor( inputData );
        }

        protected virtual bool InitializeProcessor( TSource inputData ) => true;

        protected virtual bool FinalizeProcessor( TSource inputData )
        {
            DbContext.SaveChanges();

            return true;
        }

        protected abstract bool ProcessInternal( TSource inputData );

        bool IRoslynProcessor.Process( object inputData )
        {
            if( inputData is TSource castData )
                return Process( castData );

            Logger.Error<Type, Type>( "Needed a {0} but was given a {1}", typeof( TSource ), inputData.GetType() );

            return false;
        }

        public bool Equals(BaseProcessor<TEntity, TSource>? other)
        {
            if (other == null)
                return false;

            return other.SupportedType == SupportedType;
        }
    }
}