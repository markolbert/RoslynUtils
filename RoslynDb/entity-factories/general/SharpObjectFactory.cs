using System.Linq;
using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn.Deprecated
{
    public class SharpObjectFactory : ISharpObjectFactory
    {
        private readonly RoslynDbContext _dbContext;
        private readonly ISymbolNamer _symbolNamer;
        private readonly ISharpObjectTypeMapper _sharpObjTypeMapper;
        private readonly IJ4JLogger _logger;

        public SharpObjectFactory(
            RoslynDbContext dbContext,
            ISymbolNamer symbolNamer,
            ISharpObjectTypeMapper sharpObjTypeMapper,
            IJ4JLogger logger )
        {
            _dbContext = dbContext;
            _symbolNamer = symbolNamer;
            _sharpObjTypeMapper = sharpObjTypeMapper;

            _logger = logger;
            _logger.SetLoggedType(this.GetType());
        }

        public bool Load( ISymbol symbol, out SharpObjectInfo? result, bool createIfMissing = false )
        {
            result = null;

            var fqn = _symbolNamer.GetFullyQualifiedName(symbol);
            var type = _sharpObjTypeMapper[ symbol ];

            if( type == SharpObjectType.Unknown )
            {
                _logger.Error<string>("Unknown SharpObjectType '{0}'", fqn  );
                return false;
            }

            var sharpObj = _dbContext.SharpObjects.FirstOrDefault( x => x.FullyQualifiedName == fqn );
            var existing = sharpObj != null;

            if( sharpObj == null && createIfMissing )
            {
                sharpObj = new SharpObject
                {
                    FullyQualifiedName = fqn,
                    Name = _symbolNamer.GetName( symbol ),
                };

                _dbContext.SharpObjects.Add( sharpObj );
            }

            if( sharpObj != null )
                result = new SharpObjectInfo
                {
                    FullyQualifiedName = fqn,
                    IsNew = !existing,
                    Name = _symbolNamer.GetName( symbol ),
                    SharpObject = sharpObj,
                    Symbol = symbol,
                    Type = type
                };

            return result != null;
        }
    }
}