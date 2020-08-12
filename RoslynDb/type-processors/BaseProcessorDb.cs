using System;
using System.Linq;
using J4JSoftware.Logging;
using J4JSoftware.Roslyn.entities;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;

namespace J4JSoftware.Roslyn
{
    public abstract class BaseProcessorDb<TSource> : AtomicProcessor<TSource>
    {
        private readonly RoslynDbContext _dbContext;

        protected BaseProcessorDb(
            RoslynDbContext dbContext,
            ISymbolInfo symbolInfo,
            IJ4JLogger logger
        )
        : base( logger )
        {
            _dbContext = dbContext;
            SymbolInfo = symbolInfo;
        }

        protected ISymbolInfo SymbolInfo { get; }

        protected DbSet<TRelated> GetDbSet<TRelated>()
            where TRelated : class
            => _dbContext.Set<TRelated>();

        protected bool GetByFullyQualifiedName<TEntity>(ISymbol symbol, out TEntity? result)
            where TEntity : class, IFullyQualifiedName
        {
            result = null;

            var symbolInfo = SymbolInfo.Create(symbol);

            var dbSet = _dbContext.Set<TEntity>();

            result = dbSet.FirstOrDefault(x => x.FullyQualifiedName == symbolInfo.SymbolName);

            if (result == null)
                Logger.Error<Type, string>("Couldn't find instance of {0} in database for symbol {1}",
                    typeof(TEntity),
                    symbolInfo.SymbolName);

            return result != null;
        }

        protected override bool FinalizeProcessor( TSource inputData )
        {
            if( !base.FinalizeProcessor( inputData ) )
                return false;

            _dbContext.SaveChanges();

            return true;
        }
    }
}