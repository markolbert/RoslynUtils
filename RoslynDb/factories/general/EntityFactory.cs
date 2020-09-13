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

        public EntityFactories? Factories { get; set; }

        public bool CanCreate<T>()
            where T : ISharpObject => typeof(T) == typeof(TEntity);

        public bool IsAssignableTo<T>()
            where T : ISharpObject => typeof(T).IsAssignableFrom( typeof(TEntity) );

        public bool CanProcess( ISymbol? symbol ) => GetEntitySymbol( symbol, out _ );

        public bool Get( ISymbol? symbol, out TEntity? result )
        {
            result = null;

            if( !ValidateConfiguration( symbol, out TSymbol? entitySymbol, out var uniqueName ) )
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
            result = null;

            if( !ValidateConfiguration( symbol, out var entitySymbol, out var uniqueName ) )
                return false;

            if (!Factories!.CreateSharpObject(entitySymbol!, out var sharpObj))
                return false;

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

        private bool ValidateConfiguration(ISymbol? symbol, out TSymbol? symbolResult, out string? uniqueName )
        {
            symbolResult = null;
            uniqueName = null;

            if (symbol == null)
                return false;

            if (Factories == null)
            {
                Logger.Error("IEntityFactories is undefined");
                return false;
            }

            var type = Factories.GetSharpObjectType<TEntity>();

            if (type == SharpObjectType.Unknown)
            {
                Logger.Error<Type>("Unknown SharpObjectType ({0})", typeof(TEntity));
                return false;
            }

            if (!GetEntitySymbol(symbol, out var entitySymbol))
            {
                Logger.Error<string>("Couldn't extract required symbol from '{0}'", Factories.GetFullName(symbol));
                return false;
            }

            if (!Factories!.GetUniqueName(entitySymbol!, out var name))
            {
                Logger.Error<string>("Couldn't create unique name for '{0}'",
                    Factories.GetFullName(entitySymbol!));

                return false;
            }

            if (!ValidateEntitySymbol(entitySymbol!))
                return false;

            symbolResult = entitySymbol;
            uniqueName = name;

            return true;
        }

        protected abstract bool GetEntitySymbol( ISymbol? symbol, out TSymbol? result );
        protected abstract bool CreateNewEntity( TSymbol symbol, out TEntity? result );

        protected virtual bool ValidateEntitySymbol( TSymbol symbol ) => true;

        protected virtual bool ConfigureEntity( TSymbol symbol, TEntity newEntity ) => true;

        bool IEntityFactory.Get(ISymbol? symbol, out ISharpObject? result)
        {
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
            result = null;

            if (symbol == null)
                return false;

            if (!Create(symbol, out var innerResult))
                return false;

            result = innerResult;

            return true;
        }
    }
}