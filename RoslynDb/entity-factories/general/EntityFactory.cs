using System;
using System.Linq;
using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;

namespace J4JSoftware.Roslyn
{
    // needs to be part of an IEntityFactories collection to function
    // local properties of created entities are initialized. Relationship properties
    // are not.
    public abstract class EntityFactory<TSymbol, TEntity> : IEntityFactory<TEntity>
        where TSymbol : class, ISymbol
        where TEntity : class, ISharpObject
    {
        protected EntityFactory( IJ4JLogger logger )
        {
            Logger = logger;
            Logger.SetLoggedType( this.GetType() );
        }

        protected IJ4JLogger Logger { get; }

        public IEntityFactories? Factories { get; set; }
        public Type EntityType => typeof(TEntity);

        public bool CanProcess( ISymbol symbol ) => GetEntitySymbol( symbol, out _ );

        public bool Retrieve( ISymbol symbol, out TEntity? result, bool createIfMissing = false )
        {
            result = null;

            if( Factories == null )
            {
                Logger.Error( "IEntityFactories is undefined" );
                return false;
            }

            if( !GetEntitySymbol( symbol, out var entitySymbol ) )
            {
                Logger.Error<string>( "Couldn't extract required symbol from '{0}'",
                    Factories.GetFullyQualifiedName( symbol ) );

                return false;
            }

            var fqn = Factories.GetFullyQualifiedName( entitySymbol! );

            if( !ValidateEntitySymbol( entitySymbol! ) )
                return false;

            var type = Factories.SharpObjectTypeMapper[ entitySymbol! ];

            if( type == SharpObjectType.Unknown )
            {
                Logger.Error<string>( "Unknown SharpObjectType '{0}'", fqn );
                return false;
            }

            if( !Factories.RetrieveSharpObject( entitySymbol!, out var sharpObj, createIfMissing ) )
                return false;

            var entities = Factories.DbContext.Set<TEntity>();

            result = entities
                .Include( x => x.SharpObject )
                .FirstOrDefault( x => x.SharpObject.FullyQualifiedName == fqn );

            if( result != null )
                return true;

            if( !createIfMissing || !CreateNewEntity( entitySymbol!, out var newEntity ) )
                return false;

            result = newEntity;

            if( !ConfigureEntity( entitySymbol!, result! ) )
                return false;

            if (sharpObj!.ID == 0)
                result!.SharpObject = sharpObj;
            else result!.SharpObjectID = sharpObj.ID;

            entities.Add( result );

            return true;
        }

        protected abstract bool GetEntitySymbol( ISymbol symbol, out TSymbol? result );
        protected abstract bool CreateNewEntity( TSymbol symbol, out TEntity? result );

        protected virtual bool ValidateEntitySymbol( TSymbol symbol ) => true;

        protected virtual bool ConfigureEntity( TSymbol symbol, TEntity newEntity ) => true;

        bool IEntityFactory.Retrieve( ISymbol symbol, out ISharpObject? result, bool createIfMissing )
        {
            result = null;

            if( !Retrieve( symbol, out var innerResult, createIfMissing ) )
                return false;

            result = innerResult;

            return true;
        }
    }
}