using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using J4JSoftware.Logging;
using J4JSoftware.Roslyn.entities;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;

namespace J4JSoftware.Roslyn
{
    public abstract class BaseProcessorDb<TSymbol, TSource> : AtomicProcessor<TSource>
        where TSymbol : class, ISymbol
        where TSource : IEnumerable
    {
        private readonly RoslynDbContext _dbContext;

        protected BaseProcessorDb(
            RoslynDbContext dbContext,
            ISymbolInfoFactory symbolInfo,
            IJ4JLogger logger
        )
        : base( logger )
        {
            _dbContext = dbContext;
            SymbolInfo = symbolInfo;
        }

        protected ISymbolInfoFactory SymbolInfo { get; }

        protected abstract bool ExtractSymbol( object item, out TSymbol? result );
        protected abstract bool ProcessSymbol( TSymbol symbol );

        protected override bool ProcessInternal( TSource inputData )
        {
            var allOkay = true;

            foreach( var symbol in FilterSymbols( inputData ) )
            {
                allOkay = ProcessSymbol( symbol );
            }

            return allOkay;
        }

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

        private IEnumerable<TSymbol> FilterSymbols(TSource source)
        {
            var processed = new Dictionary<string, TSymbol>();

            foreach (var item in source)
            {
                if (item == null || !ExtractSymbol(item, out var symbol))
                    continue;

                var fqn = SymbolInfo.GetFullyQualifiedName(symbol!);
                if (processed.ContainsKey(fqn))
                    continue;

                processed.Add(fqn, symbol!);

                yield return symbol!;
            }
        }
    }
}