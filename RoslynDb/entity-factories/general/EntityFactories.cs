using System.Collections.Generic;
using System.Linq;
using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;

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

        public bool CanProcess<TEntity>( ISymbol symbol )
            where TEntity : class, ISharpObject
        {
            var entityType = typeof(TEntity);

            return _factories.Any( f => f.EntityType == entityType && f.CanProcess( symbol ) );
        }

        public bool Retrieve<TEntity>( ISymbol symbol, out TEntity? result, bool createIfMissing = false )
            where TEntity : class, ISharpObject
        {
            result = null;

            var entityType = typeof(TEntity);

            var factory = _factories.FirstOrDefault( f => f.EntityType == entityType && f.CanProcess( symbol ) );

            if( factory == null )
            {
                _logger.Error(
                    "Couldn't find a factory which retrieves {0} entities and can process the provided ISymbol {1}",
                    entityType, 
                    symbol.GetType() );

                return false;
            }

            if( !factory.Retrieve( symbol, out var innerResult, createIfMissing ) )
                return false;

            result = (TEntity) innerResult!.EntityObject;

            return true;
        }

        public bool RetrieveSharpObject(ISymbol symbol, out SharpObjectInfo? result, bool createIfMissing = false)
        {
            result = null;

            var fqn = _symbolNamer.GetFullyQualifiedName(symbol);
            var type = SharpObjectTypeMapper[symbol];

            if (type == SharpObjectType.Unknown)
            {
                _logger.Error<string>("Unknown SharpObjectType '{0}'", fqn);
                return false;
            }

            var sharpObj = DbContext.SharpObjects.FirstOrDefault(x => x.FullyQualifiedName == fqn);
            var existing = sharpObj != null;

            if (sharpObj == null && createIfMissing)
            {
                sharpObj = new SharpObject
                {
                    FullyQualifiedName = fqn,
                    Name = _symbolNamer.GetName(symbol),
                };

                DbContext.SharpObjects.Add(sharpObj);
            }

            if (sharpObj != null)
                result = new SharpObjectInfo
                {
                    FullyQualifiedName = fqn,
                    IsNew = !existing,
                    Name = _symbolNamer.GetName(symbol),
                    SharpObject = sharpObj,
                    Symbol = symbol,
                    Type = type
                };

            return result != null;
        }

    }
}