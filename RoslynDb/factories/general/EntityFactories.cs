using System.Collections.Generic;
using System.Linq;
using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;

namespace J4JSoftware.Roslyn
{
    public class EntityFactories : IEntityFactories
    {
        private readonly ISymbolNamer _symbolNamer;
        private readonly List<IEntityFactory> _factories;
        private readonly IJ4JLogger _logger;

        public EntityFactories(
            RoslynDbContext dbContext,
            ISymbolNamer symbolNamer,
            ISharpObjectTypeMapper sharpObjTypeMapper,
            IEnumerable<IEntityFactory> factories,
            IJ4JLogger logger
        )
        {
            DbContext = dbContext;
            _symbolNamer = symbolNamer;
            SharpObjectTypeMapper = sharpObjTypeMapper;

            _factories = factories.ToList();
            _factories.ForEach( x => x.Factories = this );

            _logger = logger;
            _logger.SetLoggedType( this.GetType() );

            if (!_factories.Any())
                _logger.Error("No entity factories defined");
        }

        public string GetFullyQualifiedName( ISymbol symbol ) => _symbolNamer.GetFullyQualifiedName( symbol );

        public string GetName( ISymbol symbol ) => _symbolNamer.GetName( symbol );

        public ISharpObjectTypeMapper SharpObjectTypeMapper { get; }
        public RoslynDbContext DbContext { get; }

        public bool CanProcess<TEntity>( ISymbol symbol, bool createIfMissing )
            where TEntity : class, ISharpObject
        {
            var entityType = typeof(TEntity);

            // if we're not creating an entity a factory which will retrieve an
            // entity that derives from the target TEntity is okay
            return _factories.Any( f =>
                ( createIfMissing
                    ? f.EntityType == entityType
                    : f.EntityType == entityType || entityType.IsAssignableFrom( f.EntityType ) )
                && f.CanProcess( symbol ) );
        }

        public bool Retrieve<TEntity>( ISymbol? symbol, out TEntity? result, bool createIfMissing = false )
            where TEntity : class, ISharpObject
        {
            result = null;

            if( symbol == null )
                return false;

            var entityType = typeof(TEntity);

            // if we're not creating an entity a factory which will retrieve an
            // entity that derives from the target TEntity is okay
            var factory = _factories.FirstOrDefault( f =>
                ( createIfMissing
                    ? f.EntityType == entityType
                    : f.EntityType == entityType || entityType.IsAssignableFrom( f.EntityType ) )
                && f.CanProcess( symbol ) );

            if ( factory == null )
            {
                _logger.Error(
                    "Couldn't find a factory which retrieves {0} entities and can process the provided ISymbol {1}",
                    entityType, 
                    symbol.GetType() );

                return false;
            }

            if( !factory.Retrieve( symbol, out var innerResult, createIfMissing ) )
                return false;

            result = (TEntity) innerResult!;

            return true;
        }

        public bool RetrieveSharpObject(ISymbol symbol, out SharpObject? result, bool createIfMissing = false)
        {
            result = null;

            var fqn = _symbolNamer.GetFullyQualifiedName(symbol);
            var type = SharpObjectTypeMapper[symbol];

            if (type == SharpObjectType.Unknown)
            {
                _logger.Error<string>("Unknown SharpObjectType '{0}'", fqn);
                return false;
            }

            result = DbContext.SharpObjects.FirstOrDefault(x => x.FullyQualifiedName == fqn);

            if (result == null && createIfMissing)
            {
                result = new SharpObject { FullyQualifiedName = fqn };

                DbContext.SharpObjects.Add(result);
            }

            if( result == null ) 
                return false;

            result.Name = _symbolNamer.GetName( symbol );
            result.SharpObjectType = type;
            result.Synchronized = true;

            return true;
        }

        public void MarkUnsynchronized<TEntity>( bool saveChanges = false )
            where TEntity : class
        {
            var entityType = typeof(TEntity);

            if (typeof(ISharpObject).IsAssignableFrom(entityType))
            {
                // update the underlying DocObject
                var docObjType = SharpObjectTypeMapper[typeof(TEntity)];

                DbContext.SharpObjects.Where(x => x.SharpObjectType == docObjType)
                    .ForEachAsync(x => x.Synchronized = false);

                if( saveChanges )
                    DbContext.SaveChanges();

                return;
            }

            if (typeof(ISynchronized).IsAssignableFrom(entityType))
            {
                // update the entities directly
                var dbSet = DbContext.Set<TEntity>().Cast<ISynchronized>();

                dbSet.ForEachAsync(x => x.Synchronized = false);

                if (saveChanges)
                    DbContext.SaveChanges();

                return;
            }

            _logger.Error(
                "Attempting to mark as unsynchronized entities ({0}) that are neither an IDocObject nor an ISynchronized",
                entityType);
        }

        public void MarkSynchronized<TEntity>(TEntity entity)
            where TEntity : class, ISharpObject
        {
            // we can't mark newly-created ISharpObjects as synchronized
            // ...so they must be marked that way when they're created
            var entry = DbContext.Entry(entity);

            switch (entry.State)
            {
                case EntityState.Added:
                case EntityState.Detached:
                    return;
            }

            entry.Reference(x => x.SharpObject)
                .Load();

            entity.SharpObject.Synchronized = true;
        }

    }
}