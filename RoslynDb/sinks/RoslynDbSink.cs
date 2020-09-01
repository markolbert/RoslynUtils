﻿using System;
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
            ISymbolNamer symbolNamer,
            IJ4JLogger logger
        )
            : base( symbolNamer, logger )
        {
            _dbContext = dbContext;
            Symbols = new UniqueSymbols<TSymbol>(symbolNamer);
        }

        protected UniqueSymbols<TSymbol> Symbols { get; }

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

        protected DbSet<TRelated> GetDbSet<TRelated>()
            where TRelated : class
            => _dbContext.Set<TRelated>();

        protected bool GetByFullyQualifiedName<TEntity>( ISymbol symbol, out TEntity? result, bool createIfMissing = false )
            where TEntity : class, IFullyQualifiedName, new()
        {
            result = null;

            var fqn = SymbolNamer.GetFullyQualifiedName(symbol);

            var dbSet = _dbContext.Set<TEntity>();

            result = dbSet.FirstOrDefault( x => x.FullyQualifiedName == fqn );

            if( result == null )
            {
                if( createIfMissing )
                {
                    result = new TEntity { FullyQualifiedName = fqn };
                    dbSet.Add(result);
                }
                else
                {
                    Logger.Error<Type, string>( "Couldn't find instance of {0} in database for symbol {1}", 
                    typeof(TEntity),
                    fqn );

                    return false;
                }
            }

            // special handling for AssemblyDb to force loading of InScopeInfo property,
            // if it exists
            if (result is AssemblyDb assemblyDb)
                _dbContext.Entry(assemblyDb)
                    .Reference(x => x.InScopeInfo)
                    .Load();

            return true;
        }

        protected void MarkUnsynchronized<TEntity>()
            where TEntity : class
        {
            if( !typeof(ISynchronized).IsAssignableFrom( typeof(TEntity) ) )
                return;

            var dbSet = GetDbSet<TEntity>().Cast<ISynchronized>();

            dbSet.ForEachAsync(x => x.Synchronized = false);
        }

        protected virtual void SaveChanges() => _dbContext.SaveChanges();
    }
}