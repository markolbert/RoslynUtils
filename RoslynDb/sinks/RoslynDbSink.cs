using System;
using System.Linq;
using J4JSoftware.Logging;
using J4JSoftware.Roslyn.entities;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;

namespace J4JSoftware.Roslyn.Sinks
{
    public abstract class RoslynDbSink<TSymbol, TSink> : SymbolSink<TSymbol, TSink>
        where TSymbol : class, ISymbol
        where TSink : class, IFullyQualifiedName, new()
    {
        private readonly RoslynDbContext _dbContext;

        protected RoslynDbSink(
            RoslynDbContext dbContext,
            ISymbolInfo symbolInfo,
            IJ4JLogger logger
        )
            : base( symbolInfo, logger )
        {
            _dbContext = dbContext;
        }

        //public override bool TryGetSunkValue( TSymbol symbol, out TSink? result )
        //{
        //    var symbolName = SymbolName.GetFullyQualifiedName( symbol );

        //    if( GetByFullyQualifiedName(symbolName, out var innerResult ) )
        //    {
        //        result = innerResult;
        //        return true;
        //    }

        //    result = null;
        //    return false;
        //}

        protected DbSet<TRelated> GetDbSet<TRelated>()
            where TRelated : class
            => _dbContext.Set<TRelated>();

        protected TSink AddEntity(TSymbol symbol)
        {
            var dbSet = _dbContext.Set<TSink>();

            if( GetByFullyQualifiedName<TSink>( symbol, out var retVal ) )
                return retVal!;

            retVal = new TSink { FullyQualifiedName = SymbolInfo.GetFullyQualifiedName( symbol ) };
            dbSet.Add(retVal);

            ProcessedSymbolNames.Add( retVal.FullyQualifiedName );

            return retVal;
        }

        //protected bool GetByFullyQualifiedName( string fqn, out TSink? result )
        //{
        //    var dbSet = _dbContext.Set<TSink>();

        //    result = dbSet.FirstOrDefault( x => x.FullyQualifiedName == fqn );

        //    return result != null;
        //}

        protected bool GetByFullyQualifiedName<TEntity>( ISymbol symbol, out TEntity? result )
            where TEntity : class, IFullyQualifiedName
        {
            result = null;

            var symbolInfo = SymbolInfo.Create(symbol);

            var dbSet = _dbContext.Set<TEntity>();

            result = dbSet.FirstOrDefault( x => x.FullyQualifiedName == symbolInfo.SymbolName );

            if( result == null )
                Logger.Error<Type, string>( "Couldn't find instance of {0} in database for symbol {1}", 
                    typeof(TEntity),
                    symbolInfo.SymbolName );

            return result != null;
        }

        protected void MarkUnsynchronized<TEntity>()
            where TEntity : class
        {
            if( !typeof(ISynchronized).IsAssignableFrom( typeof(TEntity) ) )
                return;

            var dbSet = GetDbSet<TEntity>().Cast<ISynchronized>();

            dbSet.ForEachAsync(x => x.Synchronized = false);
        }

        protected TSink AddEntity( string fqn )
        {
            var dbSet = _dbContext.Set<TSink>();

            var retVal = new TSink { FullyQualifiedName = fqn };
            dbSet.Add( retVal );

            ProcessedSymbolNames.Add( fqn );

            return retVal;
        }

        protected virtual void SaveChanges() => _dbContext.SaveChanges();
    }
}