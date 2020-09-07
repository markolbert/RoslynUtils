using System.Linq;
using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;

namespace J4JSoftware.Roslyn.Sinks
{
    public abstract class RoslynDbSink<TSymbol, TSink> : SymbolSink<TSymbol, TSink>
        where TSymbol : class, ISymbol
        where TSink : class, ISharpObject, new()
    {
        private readonly RoslynDbContext _dbContext;

        protected RoslynDbSink(
            RoslynDbContext dbContext,
            ISymbolNamer symbolNamer,
            ISharpObjectTypeMapper sharpObjMapper,
            IJ4JLogger logger
        )
            : base( symbolNamer, logger )
        {
            _dbContext = dbContext;
            SharpObjectMapper = sharpObjMapper;

            Symbols = new UniqueSymbols<TSymbol>(symbolNamer);
        }

        protected UniqueSymbols<TSymbol> Symbols { get; }
        protected ISharpObjectTypeMapper SharpObjectMapper { get; }

        public override bool InitializeSink( ISyntaxWalker syntaxWalker )
        {
            if( !base.InitializeSink( syntaxWalker ) )
                return false;

            Symbols.Clear();

            return true;
        }

        public override bool OutputSymbol(ISyntaxWalker syntaxWalker, TSymbol symbol)
        {
            if (!base.OutputSymbol(syntaxWalker, symbol))
                return false;

            Symbols.Add( symbol );

            return true;
        }

        //protected DbSet<TRelated> GetDbSet<TRelated>()
        //    where TRelated : class
        //    => _dbContext.Set<TRelated>();

        //protected SharpObject? GetDocObject<TEntity>( ISymbol symbol )
        //    where TEntity : class, ISharpObject
        //{
        //    var docObjType = DocObjectMapper.GetDocObjectType<TEntity>();

        //    if( docObjType == SharpObjectType.Unknown )
        //    {
        //        Logger.Error<Type>( "Couldn't find DocObjectType for entity type '{0}'", typeof(TEntity) );
        //        return null;
        //    }

        //    var fqn = SymbolNamer.GetFullyQualifiedName(symbol);

        //    var retVal = _dbContext.SharpObjects.FirstOrDefault( x => x.SharpObjectType == docObjType && x.FullyQualifiedName == fqn );

        //    if( retVal == null )
        //        Logger.Information<Type, string>( "Couldn't find DocObject for entity type '{0}' ({1})", 
        //            typeof(TEntity),
        //            fqn );

        //    return retVal;
        //}

        //protected RelationshipObject? GetRelationshipObject<TSideOne, TSideTwo>(
        //    TSideOne sideOne,
        //    TSideTwo sideTwo,
        //    bool createIfMissing = false )
        //    where TSideOne : class, IDocObject
        //    where TSideTwo : class, IDocObject
        //{
        //    var sideOneType = DocObjectMapper.GetDocObjectType<TSideOne>();

        //    if( sideOneType == DocObjectType.Unknown )
        //    {
        //        Logger.Error("Unsupported IDocObject type {0}", typeof(TSideOne)  );
        //        return null;
        //    }

        //    var sideTwoType = DocObjectMapper.GetDocObjectType<TSideTwo>();

        //    if( sideTwoType == DocObjectType.Unknown )
        //    {
        //        Logger.Error("Unsupported IDocObject type {0}", typeof(TSideTwo));
        //        return null;
        //    }

        //    var retVal = _dbContext.RelationshipObjects
        //        .Include(x=>x.SideOne)
        //        .Include(x=>x.SideTwo)
        //        .FirstOrDefault( x => x.SideOneID == sideOne.ID 
        //                              && x.SideTwoID == sideTwo.ID);


        //    if( retVal != null && ( retVal.SideOne.Type != sideOneType || retVal.SideTwo.Type != sideTwoType ) )
        //    {
        //        Logger.Error("Mismatched RelationshipObject entities"  );
        //        return null;
        //    }

        //    if( retVal == null )
        //    {
        //        if( createIfMissing )
        //        {
        //            retVal = new RelationshipObject();

        //            if( sideOne.ID == 0 )
        //                retVal.SideOne = sideOne.DocObject;
        //            else retVal.SideOneID = sideOne.ID;

        //            if( sideTwo.ID == 0 )
        //                retVal.SideTwo = sideTwo.DocObject;
        //            else retVal.SideTwoID = sideTwo.ID;

        //            _dbContext.RelationshipObjects.Add( retVal );
        //        }
        //        else Logger.Error<string, string>("Couldn't find RelationshipObject for '{0}' and '{1}'",
        //            sideOne.ToString(),
        //            sideTwo.ToString());
        //    }

        //    return retVal;
        //}

        //protected bool GetByFullyQualifiedName<TEntity>( ISymbol symbol, out TEntity? result, bool createIfMissing = false )
        //    where TEntity : class, IFullyQualifiedName, new()
        //{
        //    result = null;

        //    var fqn = SymbolNamer.GetFullyQualifiedName(symbol);

        //    var dbSet = _dbContext.Set<TEntity>();

        //    if( result == null )
        //    {
        //        if( createIfMissing )
        //        {
        //            result = new TEntity { FullyQualifiedName = fqn };

        //            dbSet.Add(result);
        //        }
        //        else
        //        {
        //            Logger.Error<Type, string>( "Couldn't find instance of {0} in database for symbol {1}", 
        //            typeof(TEntity),
        //            fqn );

        //            return false;
        //        }
        //    }

        //    // special handling for AssemblyDb to force loading of InScopeInfo property,
        //    // if it exists
        //    if (result is AssemblyDb assemblyDb)
        //        _dbContext.Entry(assemblyDb)
        //            .Reference(x => x.InScopeInfo)
        //            .Load();

        //    return true;
        //}

        //protected bool GetByFullyQualifiedNameNG<TEntity>(ISymbol symbol, out TEntity? result, bool createIfMissing = false)
        //    where TEntity : class, IDocObject, IFullyQualifiedName, new()
        //{
        //    result = null;

        //    var fqn = SymbolNamer.GetFullyQualifiedName(symbol);

        //    var docObj = GetDocObject<TEntity>(symbol);

        //    var dbSet = _dbContext.Set<TEntity>();

        //    if (docObj != null)
        //        result = dbSet.FirstOrDefault(x => ((IDocObject)x).SharpObjectID == docObj.ID);

        //    if (result == null)
        //    {
        //        if (createIfMissing)
        //        {
        //            // create the DocObject if we need to
        //            if (docObj == null)
        //            {
        //                docObj = new DocObject
        //                {
        //                    FullyQualifiedName = SymbolNamer.GetFullyQualifiedName(symbol)
        //                };

        //                _dbContext.DocObjects.Add(docObj);
        //            }

        //            result = new TEntity { FullyQualifiedName = fqn };

        //            if( docObj.ID == 0 )
        //                result.DocObject = docObj;
        //            else result.SharpObjectID = docObj.ID;

        //            dbSet.Add(result);
        //        }
        //        else
        //        {
        //            Logger.Error<Type, string>("Couldn't find instance of {0} in database for symbol {1}",
        //                typeof(TEntity),
        //                fqn);

        //            return false;
        //        }
        //    }

        //    docObj!.Name = SymbolNamer.GetName( symbol );
        //    docObj.Synchronized = true;
        //    docObj.DocObjectType = DocObjectMapper.GetDocObjectType<TEntity>();

        //    // special handling for AssemblyDb to force loading of InScopeInfo property,
        //    // if it exists
        //    if (result is AssemblyDb assemblyDb)
        //        _dbContext.Entry(assemblyDb)
        //            .Reference(x => x.InScopeInfo)
        //            .Load();

        //    return true;
        //}

        protected void MarkUnsynchronized<TEntity>()
            where TEntity : class
        {
            var entityType = typeof(TEntity);

            if( typeof(ISharpObject).IsAssignableFrom( entityType ) )
            {
                // update the underlying DocObject
                var docObjType = SharpObjectMapper[ typeof(TEntity) ];

                _dbContext.SharpObjects.Where(x => x.SharpObjectType == docObjType)
                    .ForEachAsync(x => x.Synchronized = false);

                return;
            }

            if( typeof(ISynchronized).IsAssignableFrom( entityType ) )
            {
                // update the entities directly
                var dbSet = _dbContext.Set<TEntity>().Cast<ISynchronized>();

                dbSet.ForEachAsync(x => x.Synchronized = false);

                return;
            }

            Logger.Error(
                "Attempting to mark as unsynchronized entities ({0}) that are neither an IDocObject nor an ISynchronized",
                entityType );
        }

        protected virtual void SaveChanges() => _dbContext.SaveChanges();
    }
}