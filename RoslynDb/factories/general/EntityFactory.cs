using System;
using System.Linq;
using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;

namespace J4JSoftware.Roslyn
{
    // needs to be part of an IEntityFactories collection to function
    // local properties of created entities are initialized. Relationship properties
    // are not.
    public abstract class EntityFactory<TSymbol, TEntity> : IEntityFactory<TEntity>, IEntityFactoryInternal
        where TSymbol : class, ISymbol
        where TEntity : class, ISharpObject
    {
        protected EntityFactory(
            SharpObjectType sharpObjType,
            IJ4JLogger logger )
        {
            SharpObjectType = sharpObjType;
            EntityType = typeof(TEntity);
            SymbolType = typeof(TSymbol);

            Logger = logger;
            Logger.SetLoggedType( this.GetType() );
        }

        protected IJ4JLogger Logger { get; }

        public EntityFactories? Factories { get; private set; }

        public bool Initialized => Factories != null;

        public SharpObjectType SharpObjectType { get; }
        public Type EntityType { get; }
        public Type SymbolType { get; }

        public bool CanCreate<T>()
            where T : ISharpObject => typeof(T) == typeof(TEntity);

        public bool IsAssignableTo<T>()
            where T : ISharpObject => typeof(T).IsAssignableFrom( typeof(TEntity) );

        public bool CanProcess( ISymbol? symbol ) => GetEntitySymbol( symbol, out _ );

        public bool InDatabase( ISymbol? symbol )
        {
            if( !Initialized )
                throw new ArgumentException($"{this.GetType().Name} is not initialized");

            if( symbol == null )
                return false;

            return GetEntitySymbol( symbol, out var entitySymbol )
                   && ValidateEntitySymbol( entitySymbol! )
                   && Factories!.SharpObjectInDatabase( entitySymbol! );
        }

        public bool Get( ISymbol? symbol, out TEntity? result )
        {
            if (!Initialized)
                throw new ArgumentException($"{this.GetType().Name} is not initialized");

            result = null;

            if( symbol == null )
            {
                Logger.Information( "symbol is undefined" );
                return false;
            }

            if( !GetEntitySymbol( symbol, out var entitySymbol ) )
                return false;

            if (!ValidateEntitySymbol(entitySymbol!))
                return false;

            if( !Factories!.GetSharpObject( entitySymbol!, out var sharpObj ) )
                return false;

            var entities = Factories.DbContext.Set<TEntity>();

            result = entities
                .FirstOrDefault( x => x.SharpObjectID == sharpObj!.ID );

            if( result != null ) 
                return true;

            Logger.Error<Type, string>( "Couldn't find {0} in database for '{1}'", typeof(TEntity),
                Factories.GetFullName( symbol! ) );

            return false;
        }

        public bool Create(ISymbol? symbol, out TEntity? result)
        {
            if (!Initialized)
                throw new ArgumentException($"{this.GetType().Name} is not initialized");

            result = null;

            if (symbol == null)
            {
                Logger.Information("symbol is undefined");
                return false;
            }

            if (!GetEntitySymbol(symbol, out var entitySymbol))
                return false;

            if (!ValidateEntitySymbol(entitySymbol!))
                return false;

            if (!Factories!.CreateSharpObject(entitySymbol!, out var sharpObj))
                return false;

            if (!Factories!.GetUniqueName(entitySymbol!, out var uniqueName))
            {
                Logger.Error<string>("Couldn't create unique name for '{0}'",
                    Factories.GetFullName(entitySymbol!));

                return false;
            }

            var entities = Factories.DbContext.Set<TEntity>();

            result = entities
                .Include(x => x.SharpObject)
                .FirstOrDefault(x => x.SharpObject.FullyQualifiedName == uniqueName);

            if (result != null)
                return true;

            if (!CreateNewEntity(entitySymbol!, out var newEntity))
                return false;

            result = newEntity;

            if (!ConfigureEntity(entitySymbol!, result!))
                return false;

            if (sharpObj!.ID == 0)
                result!.SharpObject = sharpObj;
            else result!.SharpObjectID = sharpObj.ID;

            entities.Add(result);

            return true;
        }

        protected abstract bool GetEntitySymbol( ISymbol? symbol, out TSymbol? result );
        protected abstract bool CreateNewEntity( TSymbol symbol, out TEntity? result );

        protected virtual bool ValidateEntitySymbol( TSymbol symbol ) => true;

        protected virtual bool ConfigureEntity( TSymbol symbol, TEntity newEntity ) => true;

        bool IEntityFactory.Get(ISymbol? symbol, out ISharpObject? result)
        {
            if (!Initialized)
                throw new ArgumentException($"{this.GetType().Name} is not initialized");

            result = null;

            if (symbol == null)
                return false;

            if (!Get(symbol, out var innerResult))
                return false;

            result = innerResult;

            return true;
        }

        bool IEntityFactory.Create(ISymbol? symbol, out ISharpObject? result)
        {
            if (!Initialized)
                throw new ArgumentException($"{this.GetType().Name} is not initialized");

            result = null;

            if (symbol == null)
                return false;

            if (!Create(symbol, out var innerResult))
                return false;

            result = innerResult;

            return true;
        }

        void IEntityFactoryInternal.SetFactories( EntityFactories factories )
        {
            Factories = factories;
        }
    }
}