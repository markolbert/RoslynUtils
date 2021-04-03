using System;
using J4JSoftware.Logging;

namespace J4JSoftware.DocCompiler
{
    public abstract class FullyQualifiedName<TSource> : IFullyQualifiedName<TSource>
    {
        protected FullyQualifiedName(
            IJ4JLogger? logger
        )
        {
            Logger = logger;
            Logger?.SetLoggedType( GetType() );
        }

        protected IJ4JLogger? Logger { get; }

        public Type SupportedType => typeof( TSource );

        public abstract bool GetName( TSource source, out string? result );
        public abstract bool GetFullyQualifiedName( TSource source, out string? result );

        bool IFullyQualifiedName.GetName( object source, out string? result )
        {
            result = null;

            if (source is TSource castSource)
                return GetName(castSource, out result);

            Logger?.Error("Expected a {0} but got a {1}", typeof(TSource), source.GetType());

            return false;
        }

        bool IFullyQualifiedName.GetFullyQualifiedName( object source, out string? result )
        {
            result = null;

            if( source is TSource castSource )
                return GetFullyQualifiedName( castSource, out result );

            Logger?.Error( "Expected a {0} but got a {1}", typeof( TSource ), source.GetType() );

            return false;
        }
    }
}