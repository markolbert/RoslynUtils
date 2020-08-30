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
            ISymbolNamer symbolInfo,
            IJ4JLogger logger
        )
        : base( logger )
        {
            _dbContext = dbContext;
            SymbolInfo = symbolInfo;
        }

        protected ISymbolNamer SymbolInfo { get; }

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

        protected bool GetByFullyQualifiedName<TEntity>(ISymbol symbol, out TEntity? result, bool createIfMissing = false )
            where TEntity : class, IFullyQualifiedName, new()
        {
            result = null;

            var dbSet = _dbContext.Set<TEntity>();

            var fqn = SymbolInfo.GetFullyQualifiedName( symbol );

            result = dbSet.FirstOrDefault(x => x.FullyQualifiedName == fqn);

            if (result == null)
            {
                if( createIfMissing )
                {
                    result = new TEntity { FullyQualifiedName = fqn };
                    dbSet.Add( result );
                }
                else
                {
                    Logger.Error<Type, string>("Couldn't find instance of {0} in database for symbol {1}",
                    typeof(TEntity),
                    fqn);

                    return false;
                }
            }

            // special handling for AssemblyDb to force loading of InScopeInfo property,
            // if it exists
            if( result is AssemblyDb assemblyDb )
                _dbContext.Entry(assemblyDb  )
                    .Reference(x=>x.InScopeInfo)
                    .Load();

            return true;
        }

        protected TypeDb? GetTypeByFullyQualifiedName( ITypeSymbol symbol, bool createIfMissing = false )
        {
            return symbol switch
            {
                INamedTypeSymbol ntSymbol => GetFixedType( ntSymbol, createIfMissing ),
                IArrayTypeSymbol arraySymbol => GetArrayType( arraySymbol, createIfMissing ),
                ITypeParameterSymbol tpSymbol => GetParametricType( tpSymbol, createIfMissing ),
                _ => null
            };
        }

        protected TypeDb? GetParametricTypeDeclaringType( ITypeParameterSymbol symbol )
        {
            var fqn = SymbolInfo.GetFullyQualifiedName( symbol );

            if( symbol.DeclaringType == null )
            {
                Logger.Error<string>( "ITypeParameterSymbol '{0}' does not have a DeclaringType property", fqn );
                return null;
            }

            if( GetByFullyQualifiedName<GenericTypeDb>( symbol.DeclaringType, out var genericDb ) )
                return genericDb!;
            else
            {
                if( GetByFullyQualifiedName<FixedTypeDb>( symbol.DeclaringType, out var fixedDb ) )
                    return fixedDb!;
            }

            Logger.Error<string>( "ITypeParameterSymbol.DeclaringType '{0}' not defined in the database", fqn );

            return null;
        }

        private TypeDb? GetFixedType( INamedTypeSymbol symbol, bool createIfMissing, string? fqn = null )
        {
            fqn ??= SymbolInfo.GetFullyQualifiedName( symbol );

            if( symbol.IsGenericType )
            {
                var genericDb = GetDbSet<GenericTypeDb>().FirstOrDefault(x => x.FullyQualifiedName == fqn);

                if( genericDb == null && createIfMissing )
                {
                    genericDb = new GenericTypeDb { FullyQualifiedName = fqn };
                    var genericTypes = GetDbSet<GenericTypeDb>();
                    genericTypes.Add( genericDb );
                }

                return genericDb;
            }

            var fixedDb = GetDbSet<FixedTypeDb>().FirstOrDefault(x => x.FullyQualifiedName == fqn);

            if( fixedDb == null && createIfMissing )
            {
                fixedDb = new FixedTypeDb { FullyQualifiedName = fqn };
                var fixedTypes = GetDbSet<FixedTypeDb>();
                fixedTypes.Add(fixedDb);
            }

            return fixedDb;
        }

        private TypeDb? GetArrayType( IArrayTypeSymbol symbol, bool createIfMissing, string? fqn = null )
        {
            fqn ??= SymbolInfo.GetFullyQualifiedName(symbol);

            switch (symbol.ElementType)
            {
                case INamedTypeSymbol ntSymbol:
                    return GetFixedType(ntSymbol, createIfMissing, fqn);

                case IArrayTypeSymbol arraySymbol:
                    return GetArrayType(arraySymbol, createIfMissing, fqn);

                case ITypeParameterSymbol tpSymbol:
                    return GetParametricType( tpSymbol, createIfMissing, fqn );

                default:
                    Logger.Error<string, TypeKind>("Unsupported array element type '{0}' ({1})",
                        symbol.Name,
                        symbol.TypeKind);

                    return null;
            }
        }

        private TypeDb? GetParametricType( ITypeParameterSymbol symbol, bool createIfMissing, string? fqn = null )
        {
            var containingTypeDb = GetParametricTypeDeclaringType( symbol );

            if( containingTypeDb == null )
                return null;

            fqn ??= SymbolInfo.GetFullyQualifiedName(symbol);

            var retVal = GetDbSet<ParametricTypeDb>().FirstOrDefault( x => x.FullyQualifiedName == fqn );

            if( retVal == null && createIfMissing )
            {
                retVal = new ParametricTypeDb { FullyQualifiedName = fqn };

                var parametricTypes = GetDbSet<ParametricTypeDb>();
                parametricTypes.Add(retVal);

                if (containingTypeDb.ID == 0)
                    retVal.ContainingType = containingTypeDb;
                else retVal.ContainingTypeID = containingTypeDb.ID;
            }

            return retVal;
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