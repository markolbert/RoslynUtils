using System;
using J4JSoftware.Logging;

namespace J4JSoftware.Roslyn
{
    public abstract class BaseProcessor<TEntity, TSource> : IRoslynProcessor<TSource>
    {
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

        public abstract bool Process( TSource inputData );

        bool IRoslynProcessor.Process( object inputData )
        {
            if( inputData is TSource castData )
                return Process( castData );

            Logger.Error<Type, Type>( "Needed a {0} but was given a {1}", typeof( TSource ), inputData.GetType() );

            return false;
        }
    }
}