using System.Collections.Generic;
using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn
{
    public abstract class BaseProcessorDb<TSource, TResult> : AtomicProcessor<TSource>
        where TResult : class, ISymbol
        where TSource : class, ISymbol
    {
        protected BaseProcessorDb(
            RoslynDbContext dbContext,
            IEntityFactories factories,
            ISymbolNamer symbolNamer,
            ISharpObjectTypeMapper sharpObjMapper,
            IJ4JLogger logger
        )
        : base( logger )
        {
            DbContext = dbContext;
            EntityFactories = factories;
            SharpObjectMapper = sharpObjMapper;
            SymbolNamer = symbolNamer;
        }

        protected RoslynDbContext DbContext { get; }
        protected ISymbolNamer SymbolNamer { get; }
        protected ISharpObjectTypeMapper SharpObjectMapper { get; }
        protected IEntityFactories EntityFactories { get; }
        
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

        protected override bool FinalizeProcessor(IEnumerable<TSource> inputData)
        {
            if (!base.FinalizeProcessor(inputData))
                return false;

            DbContext.SaveChanges();

            return true;
        }

        protected bool RetrieveAssembly( IAssemblySymbol symbol, out AssemblyDb? result )
        {
            result = null;

            if (!EntityFactories.Retrieve<AssemblyDb>(symbol, out var retVal))
            {
                Logger.Error<string>("Couldn't retrieve AssemblyDb entity for '{0}'",
                    EntityFactories.GetFullyQualifiedName(symbol.ContainingAssembly));

                return false;
            }

            result = retVal;

            return true;
        }

        protected bool RetrieveNamespace(INamespaceSymbol symbol, out NamespaceDb? result)
        {
            result = null;

            if (!EntityFactories.Retrieve<NamespaceDb>(symbol, out var retVal))
            {
                Logger.Error<string>("Couldn't retrieve AssemblyDb entity for '{0}'",
                    EntityFactories.GetFullyQualifiedName(symbol.ContainingAssembly));

                return false;
            }

            result = retVal;

            return true;
        }

        protected void MarkSynchronized<TEntity>( TEntity entity )
            where TEntity : class, ISharpObject
        {
            DbContext.Entry( entity )
                .Reference(x=>x.SharpObject)
                .Load();

            entity.SharpObject.Synchronized = true;
        }

        //protected DbSet<TRelated> GetDbSet<TRelated>()
        //    where TRelated : class
        //    => DbContext.Set<TRelated>();

        //protected TypeDb? GetTypeByFullyQualifiedName( ITypeSymbol symbol, bool createIfMissing = false )
        //{
        //    return symbol switch
        //    {
        //        INamedTypeSymbol ntSymbol => ntSymbol.IsGenericType switch
        //        {
        //            true => GetGenericType( ntSymbol, createIfMissing ),
        //            _ => GetFixedType( ntSymbol, createIfMissing )
        //        },
        //        IArrayTypeSymbol arraySymbol => GetTypeByFullyQualifiedName( arraySymbol, createIfMissing ),
        //        ITypeParameterSymbol tpSymbol => GetTypeByFullyQualifiedName( tpSymbol, createIfMissing ),
        //        _ => null
        //    };
        //}

        //protected TypeDb? GetTypeByFullyQualifiedName( INamedTypeSymbol symbol, bool createIfMissing = false )
        //{
        //    return symbol.IsGenericType switch
        //    {
        //        true => GetGenericType( symbol, createIfMissing ),
        //        _ => GetFixedType( symbol, createIfMissing )
        //    };
        //}

        //protected TypeDb? GetTypeByFullyQualifiedName( 
        //    IArrayTypeSymbol symbol, 
        //    bool createIfMissing,
        //    string? fqn = null )
        //{
        //    fqn ??= SymbolNamer.GetFullyQualifiedName( symbol );

        //    return symbol.ElementType switch
        //    {
        //        INamedTypeSymbol ntSymbol => ntSymbol.IsGenericType switch
        //        {
        //            true => GetGenericType( ntSymbol, createIfMissing, fqn ),
        //            _ => GetFixedType( ntSymbol, createIfMissing, fqn )
        //        },
        //        IArrayTypeSymbol arraySymbol => GetTypeByFullyQualifiedName( arraySymbol, createIfMissing, fqn ),
        //        ITypeParameterSymbol tpSymbol => GetTypeByFullyQualifiedName( tpSymbol, createIfMissing, fqn ),
        //        _ => unhandled()
        //    };

        //    TypeDb? unhandled()
        //    {
        //        Logger.Error<string, TypeKind>( "Unsupported array element type '{0}' ({1})",
        //            symbol.Name,
        //            symbol.TypeKind );

        //        return null;
        //    }
        //}

        //protected ParametricTypeDb? GetTypeByFullyQualifiedName(
        //    ITypeParameterSymbol symbol,
        //    bool createIfMissing,
        //    string? fqn = null)
        //{
        //    fqn ??= SymbolNamer.GetFullyQualifiedName(symbol);

        //    var sharpObj = GetDocObject(symbol, createIfMissing);

        //    if (sharpObj == null)
        //    {
        //        Logger.Error<string>("Couldn't find or create DocObject for {0}", fqn);
        //        return null;
        //    }

        //    var parametricTypes = GetDbSet<ParametricTypeDb>();

        //    var retVal = parametricTypes
        //        .FirstOrDefault(x => x.SharpObjectID == sharpObj.ID);

        //    if (retVal != null || !createIfMissing)
        //        return retVal;

        //    if( symbol.DeclaringType != null )
        //    {
        //        var containingTypeDb = GetTypeByFullyQualifiedName( symbol.DeclaringType );

        //        if( containingTypeDb == null )
        //            return null;

        //        if( !GetByFullyQualifiedName<ITypeParameterSymbol, TypeParametricTypeDb>( 
        //            symbol, 
        //            out var result,
        //            true ) )
        //            return null;

        //        result!.ContainingTypeID = containingTypeDb.SharpObjectID;

        //        retVal = result;
        //    }

        //    if( symbol.DeclaringMethod != null )
        //        retVal = CreateMethodParametricType( symbol );

        //    if( retVal != null )
        //    {
        //        parametricTypes.Add(retVal);

        //        return retVal;
        //    }

        //    Logger.Error<string>("Unsupported parametric type container ({0})", fqn);

        //    return null;
        //}

        //protected virtual ParametricTypeDb? CreateMethodParametricType( ITypeParameterSymbol symbol )
        //{
        //    if( symbol.DeclaringMethod == null )
        //    {
        //        Logger.Error<string>( "ITypeParameterSymbol '{0}' does not have a DeclaringMethod",
        //            SymbolNamer.GetFullyQualifiedName( symbol ) );

        //        return null;
        //    }

        //    if( !GetByFullyQualifiedName<IMethodSymbol, MethodDb>( symbol.DeclaringMethod, out var methodDb ) )
        //        return null;

        //    if( !GetByFullyQualifiedName<ITypeParameterSymbol, MethodParametricTypeDb>( symbol, out var retVal, true ) )
        //        return null;

        //    retVal!.ContainingMethodID = methodDb!.SharpObjectID;

        //    return retVal;
        //}

        //protected void SaveChanges() => DbContext.SaveChanges();

        //protected SharpObject? GetDocObject(ISymbol symbol, bool createIfNeeded = false )
        //{
        //    var fqn = SymbolNamer.GetFullyQualifiedName(symbol);

        //    var sharpType = SharpObjectMapper[symbol];

        //    if (sharpType == SharpObjectType.Unknown)
        //    {
        //        Logger.Error<SharpObjectType>("Couldn't find DocObjectType for entity type '{0}'", sharpType);
        //        return null;
        //    }

        //    var retVal = _dbContext.SharpObjects.FirstOrDefault(x => x.SharpObjectType == sharpType && x.FullyQualifiedName == fqn);

        //    if( retVal == null )
        //    {
        //        if( createIfNeeded )
        //        {
        //            retVal = new SharpObject
        //            {
        //                FullyQualifiedName = SymbolNamer.GetFullyQualifiedName( symbol )
        //            };

        //            _dbContext.SharpObjects.Add( retVal );

        //            retVal!.Name = SymbolNamer.GetName( symbol );
        //            retVal.Synchronized = true;
        //            retVal.SharpObjectType = sharpType;
        //        }
        //        else
        //            Logger.Information<SharpObjectType, string>( "Couldn't find DocObject for entity type '{0}' ({1})",
        //                sharpType,
        //                fqn );
        //    }

        //    return retVal;
        //}

        //private void UpdateDocObjectReference( ISharpObject target, SharpObject sharpObj )
        //{
        //    if (sharpObj.ID == 0)
        //        target.SharpObject = sharpObj;
        //    else target.SharpObjectID = sharpObj.ID;
        //}

        //protected bool GetByFullyQualifiedName<TSymbol, TEntity>(TSymbol symbol, out TEntity? result, bool createIfMissing = false)
        //    where TSymbol : ISymbol
        //    where TEntity : class, ISharpObject, new()
        //{
        //    result = null;

        //    var fqn = SymbolNamer.GetFullyQualifiedName(symbol);

        //    var docObj = GetDocObject(symbol, createIfMissing);

        //    if( docObj == null )
        //    {
        //        Logger.Error<string>("Couldn't find or create DocObject for {0}", fqn);
        //        return false;
        //    }

        //    var dbSet = _dbContext.Set<TEntity>();

        //    result = dbSet.FirstOrDefault(x => x.SharpObjectID == docObj.ID);

        //    if (result == null)
        //    {
        //        if (createIfMissing)
        //        {
        //            result = new TEntity();

        //            UpdateDocObjectReference(result, docObj);

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

        //    // special handling for AssemblyDb to force loading of InScopeInfo property,
        //    // if it exists
        //    if (result is AssemblyDb assemblyDb)
        //        _dbContext.Entry(assemblyDb)
        //            .Reference(x => x.InScopeInfo)
        //            .Load();

        //    return true;
        //}

        // symbol is assumed to be a non-generic type
        //private FixedTypeDb? GetFixedType(INamedTypeSymbol symbol, bool createIfMissing, string? fqn = null)
        //{
        //    fqn ??= SymbolNamer.GetFullyQualifiedName(symbol);

        //    var docObj = GetDocObject(symbol, true);

        //    if (docObj == null)
        //    {
        //        Logger.Error<string>("Couldn't find or create DocObject for {0}", fqn);
        //        return null;
        //    }

        //    var retVal = GetDbSet<FixedTypeDb>().FirstOrDefault(x => x.SharpObjectID == docObj.ID);

        //    if( retVal != null || !createIfMissing ) 
        //        return retVal;

        //    retVal = new FixedTypeDb();

        //    UpdateDocObjectReference(retVal, docObj);

        //    var fixedTypes = GetDbSet<FixedTypeDb>();

        //    fixedTypes.Add(retVal);

        //    return retVal;
        //}

        //// symbol is assumed to be a generic type
        //private GenericTypeDb? GetGenericType( INamedTypeSymbol symbol, bool createIfMissing, string? fqn = null )
        //{
        //    fqn ??= SymbolNamer.GetFullyQualifiedName( symbol );

        //    var docObj = GetDocObject( symbol, true );

        //    if( docObj == null )
        //    {
        //        Logger.Error<string>( "Couldn't find or create DocObject for {0}", fqn );
        //        return null;
        //    }

        //    var retVal = GetDbSet<GenericTypeDb>().FirstOrDefault( x => x.SharpObjectID == docObj.ID );

        //    if( retVal != null || !createIfMissing ) 
        //        return retVal;

        //    retVal = new GenericTypeDb();

        //    UpdateDocObjectReference( retVal, docObj );

        //    var genericTypes = GetDbSet<GenericTypeDb>();

        //    genericTypes.Add( retVal );

        //    return retVal;
        //}

        //protected object? GetParametricTypeContainer( ITypeParameterSymbol symbol )
        //{
        //    if( symbol.DeclaringType != null )
        //        return GetTypeByFullyQualifiedName( symbol.DeclaringType );
        //    else
        //    {
        //        if( symbol.DeclaringMethod != null
        //            && GetByFullyQualifiedName<IMethodSymbol, MethodDb>( symbol.DeclaringMethod, out var result ) )
        //            return result;
        //    }

        //    return null;
        //}

        //// symbol.DeclaringType is assumed to be non-null
        //private ImplementableTypeDb? GetTypeContainer(ITypeParameterSymbol symbol)
        //{
        //    if (GetByFullyQualifiedName<INamedTypeSymbol, GenericTypeDb>(symbol.DeclaringType!, out var genericDb))
        //        return genericDb!;
        //    else
        //    {
        //        if (GetByFullyQualifiedName<INamedTypeSymbol, FixedTypeDb>(symbol.DeclaringType!, out var fixedDb))
        //            return fixedDb!;
        //    }

        //    Logger.Error<string>( "DeclaringType of ITypeParameterSymbol '{0}' not found in the database",
        //        SymbolNamer.GetFullyQualifiedName( symbol ) );

        //    return null;
        //}

        // symbol.DeclaringMethod is assumed to be non-null
        //private MethodDb? GetMethodContainer(ITypeParameterSymbol symbol)
        //{
        //    if (GetByFullyQualifiedName<IMethodSymbol, MethodDb>(symbol.DeclaringMethod!, out var placeHolderDb, true))
        //        return placeHolderDb!;

        //    Logger.Error<string>( "DeclaringMethod of ITypeParameterSymbol '{0}' not found in the database",
        //        SymbolNamer.GetFullyQualifiedName( symbol ) );

        //    return null;
        //}

        //private TypeParametricTypeDb? CreateTypeParametricType( INamedTypeSymbol symbol )
        //{
        //    var parametricTypes = GetDbSet<TypeParametricTypeDb>();

        //    var retVal = parametricTypes
        //        .FirstOrDefault( x => x.SharpObjectID == sharpObj.ID );

        //    if( retVal != null || !createIfMissing ) 
        //        return retVal;

        //    retVal = new TypeParametricTypeDb();

        //    UpdateDocObjectReference( retVal, docObj );

        //    parametricTypes.Add( retVal );

        //    return retVal;
        //}

        private IEnumerable<TResult> FilterSymbols(IEnumerable<TSource> source)
        {
            var processed = new Dictionary<string, TResult>();

            foreach (var item in source)
            {
                if (item == null )
                    continue;

                foreach( var symbol in ExtractSymbols(item) )
                {
                    var fqn = SymbolNamer.GetFullyQualifiedName(symbol!);

                    if (processed.ContainsKey(fqn))
                        continue;

                    processed.Add(fqn, symbol!);

                    yield return symbol!;
                }
            }
        }
    }
}