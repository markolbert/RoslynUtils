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
    public abstract class BaseProcessorDb<TSource, TResult> : AtomicProcessor<IEnumerable<TSource>>
        where TResult : class, ISymbol
        where TSource : class, ISymbol
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

        protected abstract IEnumerable<TResult> ExtractSymbols( object item );
        protected abstract bool ProcessSymbol( TResult symbol );

        protected override bool ProcessInternal( IEnumerable<TSource> inputData )
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

            var dbSet = _dbContext.Set<TEntity>();

            var fqn = SymbolInfo.GetFullyQualifiedName( symbol );

            result = dbSet.FirstOrDefault(x => x.FullyQualifiedName == fqn);

            if (result == null)
                Logger.Error<Type, string>("Couldn't find instance of {0} in database for symbol {1}",
                    typeof(TEntity),
                    fqn);

            // special handling for AssemblyDb to force loading of InScopeInfo property,
            // if it exists
            if( result is AssemblyDb assemblyDb )
                _dbContext.Entry(assemblyDb  )
                    .Reference(x=>x.InScopeInfo)
                    .Load();

            return result != null;
        }

        protected override bool FinalizeProcessor( IEnumerable<TSource> inputData )
        {
            if( !base.FinalizeProcessor( inputData ) )
                return false;

            SaveChanges();

            return true;
        }

        protected void SaveChanges() => _dbContext.SaveChanges();

        private IEnumerable<TResult> FilterSymbols(IEnumerable<TSource> source)
        {
            var processed = new Dictionary<string, TResult>();

            foreach (var item in source)
            {
                if (item == null )
                    continue;

                foreach( var symbol in ExtractSymbols(item) )
                {
                    var fqn = SymbolInfo.GetFullyQualifiedName(symbol!);
                    if (processed.ContainsKey(fqn))
                        continue;

                    processed.Add(fqn, symbol!);

                    yield return symbol!;
                }
            }
        }
    }
}