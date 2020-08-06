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
            ISymbolName symbolName,
            IJ4JLogger logger
        )
            : base( symbolName, logger )
        {
            _dbContext = dbContext;
        }

        public override bool TryGetSunkValue( TSymbol symbol, out TSink? result )
        {
            var symbolName = SymbolName.GetFullyQualifiedName( symbol );

            if( GetByFullyQualifiedName(symbolName, out var innerResult ) )
            {
                result = innerResult;
                return true;
            }

            result = null;
            return false;
        }

        protected DbSet<TRelated> GetDbSet<TRelated>()
            where TRelated : class
            => _dbContext.Set<TRelated>();

        protected bool GetByFullyQualifiedName( string fqn, out TSink? result )
        {
            var dbSet = _dbContext.Set<TSink>();

            result = dbSet.FirstOrDefault( x => x.FullyQualifiedName == fqn );

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