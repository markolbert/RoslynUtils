﻿using System;
using System.Collections.Generic;
using System.Linq;
using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;

namespace J4JSoftware.Roslyn
{
    public class RoslynDataLayer : IRoslynDataLayer
    {
        private readonly RoslynDbContext _dbContext;
        private readonly IJ4JLogger _logger;

        public RoslynDataLayer(
            RoslynDbContext dbContext,
            IJ4JLogger logger
        )
        {
            _dbContext = dbContext;

            _logger = logger;
            _logger.SetLoggedType( this.GetType() );
        }

        #region clearing synchronization

        public void MarkUnsynchronized<TEntity>(bool saveChanges = true)
            where TEntity : class, ISynchronized
        {
            var entityType = typeof(TEntity);

            // update the entities directly
            var dbSet = _dbContext.Set<TEntity>().Cast<ISynchronized>();

            dbSet.ForEachAsync(x => x.Synchronized = false);

            if (saveChanges)
                _dbContext.SaveChanges();
        }

        public void MarkSharpObjectUnsynchronized<TEntity>(bool saveChanges = true)
            where TEntity : class, ISharpObject
        {
            var entityType = typeof(TEntity);

            // update the entities directly
            var dbSet = _dbContext.Set<TEntity>()
                .Include(x=>x.SharpObject)
                .Cast<ISharpObject>();

            dbSet.ForEachAsync(x => x.SharpObject.Synchronized = false);

            if (saveChanges)
                _dbContext.SaveChanges();
        }

        #endregion

        public void SaveChanges() => _dbContext.SaveChanges();

        #region SharpObject

        public bool SharpObjectInDatabase<TEntity>( ISymbol symbol )
            where TEntity : class, ISharpObject
        {
            var fqn = symbol.GetUniqueName();

            var sharpObj = _dbContext.SharpObjects.FirstOrDefault( x => x.FullyQualifiedName == fqn );

            return sharpObj != null;
        }

        public ISharpObject? GetDescendantEntity<TEntity>(ISymbol symbol)
            where TEntity : class, ISharpObject
        {
            var fqn = symbol.GetUniqueName();

            var sharpObj = _dbContext.SharpObjects.FirstOrDefault(x => x.FullyQualifiedName == fqn);

            if (sharpObj == null)
                return null;

            switch( sharpObj.SharpObjectType )
            {
                case SharpObjectType.Assembly:
                    _dbContext.Entry( sharpObj )
                        .Reference( x => x.Assembly )
                        .Load();

                    return sharpObj.Assembly;

                case SharpObjectType.Namespace:
                    _dbContext.Entry(sharpObj)
                        .Reference(x => x.Namespace)
                        .Load();

                    return sharpObj.Namespace;

                case SharpObjectType.FixedType:
                    _dbContext.Entry(sharpObj)
                        .Reference(x => x.FixedType)
                        .Load();

                    return sharpObj.Namespace;

                case SharpObjectType.GenericType:
                    _dbContext.Entry(sharpObj)
                        .Reference(x => x.GenericType)
                        .Load();

                    return sharpObj.GenericType;

                case SharpObjectType.ParametricType:
                    _dbContext.Entry(sharpObj)
                        .Reference(x => x.ParametricType)
                        .Load();

                    return sharpObj.ParametricType;

                case SharpObjectType.ParametricMethodType:
                    _dbContext.Entry(sharpObj)
                        .Reference(x => x.ParametricMethodType)
                        .Load();

                    return sharpObj.ParametricMethodType;

                case SharpObjectType.ArrayType:
                    _dbContext.Entry(sharpObj)
                        .Reference(x => x.ArrayType)
                        .Load();

                    return sharpObj.ArrayType;

                case SharpObjectType.Method:
                    _dbContext.Entry(sharpObj)
                        .Reference(x => x.Method)
                        .Load();

                    return sharpObj.Method;

                case SharpObjectType.MethodArgument:
                    _dbContext.Entry(sharpObj)
                        .Reference(x => x.MethodArgument)
                        .Load();

                    return sharpObj.MethodArgument;

                case SharpObjectType.Property:
                    _dbContext.Entry(sharpObj)
                        .Reference(x => x.Property)
                        .Load();

                    return sharpObj.Property;

                case SharpObjectType.PropertyParameter:
                    _dbContext.Entry(sharpObj)
                        .Reference(x => x.PropertyParameter)
                        .Load();

                    return sharpObj.PropertyParameter;

                case SharpObjectType.Field:
                    _dbContext.Entry(sharpObj)
                        .Reference(x => x.Field)
                        .Load();

                    return sharpObj.Field;
            }

            _logger.Error( "Unsupported SharpObjectType {0}", sharpObj.SharpObjectType );

            return null;
        }

        #endregion

        #region Assembly

        public AssemblyDb? GetAssembly( IAssemblySymbol symbol, bool createIfMissing = false, bool updateExisting = false )
        {
            var fqn = symbol.ToUniqueName();

            var retVal = _dbContext.Assemblies
                .Include(x => x.SharpObject)
                .FirstOrDefault(x => x.SharpObject.FullyQualifiedName == fqn);

            if( retVal != null )
            {
                if( updateExisting ) 
                    UpdateAssembly( symbol, retVal );

                return retVal;
            }

            if (!createIfMissing)
            {
                _logger.Error<string>("Couldn't find AssemblyDb object for '{0}' in the database", fqn);
                return null;
            }

            retVal = new AssemblyDb
            {
                SharpObject = GetSharpObject(symbol, true, SharpObjectType.Assembly )!,
            };

            _dbContext.Assemblies.Add(retVal);

            // update after add so the entity is being tracked
            UpdateAssembly(symbol, retVal);

            return retVal;
        }

        public bool UpdateAssembly( IAssemblySymbol symbol, AssemblyDb entity )
        {
            if( !SharpObjectMatchesSymbol( symbol, entity ) )
                return false;

            entity.SharpObject.Synchronized = true;

            return true;
        }

        public InScopeAssemblyInfo? GetInScopeAssemblyInfo( 
            CompiledProject project,
            bool createIfMissing = false,
            bool updateExisting = false )
        {
            var fqn = project.AssemblySymbol.ToUniqueName();

            var assemblyDb = _dbContext.Assemblies
                .Include( x => x.SharpObject )
                .Include( x => x.InScopeInfo )
                .FirstOrDefault( x => x.SharpObject.FullyQualifiedName == fqn );

            if( assemblyDb == null )
            {
                _logger.Error<string>(
                    "Couldn't find AssemblyDb object for '{0}' in the database, can't return or create InScopeAssemblyInfo entity",
                    fqn );

                return null;
            }

            var retVal = assemblyDb.InScopeInfo;

            if (retVal != null)
            {
                if (updateExisting)
                    UpdateInScopeAssemblyInfo(project, retVal);

                return retVal;
            }

            if (!createIfMissing)
            {
                _logger.Error<string>( "Couldn't find InScopeAssemblyInfo object for '{0}' in the database", fqn );
                return null;
            }

            retVal = new InScopeAssemblyInfo
            {
                Assembly = assemblyDb
            };

            _dbContext.InScopeInfo.Add( retVal );

            // update after add so the entity is being tracked
            UpdateInScopeAssemblyInfo(project, retVal);

            return retVal;
        }

        public bool UpdateInScopeAssemblyInfo(CompiledProject project, InScopeAssemblyInfo entity)
        {
            _dbContext.Entry(entity)
                .Reference(x => x.Assembly)
                .Load();

            _dbContext.Entry(entity.Assembly)
                .Reference(x => x.SharpObject)
                .Load();

            var fqn = project.AssemblySymbol.GetUniqueName();

            if( !entity.Assembly.SharpObject.FullyQualifiedName.Equals( fqn, StringComparison.Ordinal ) )
            {
                _logger.Error<string, Type, string>( "CompiledProject '{0}' does not correspond to {1} '{2}'",
                    project.AssemblyName,
                    typeof(InScopeAssemblyInfo),
                    entity.Assembly.SharpObject.FullyQualifiedName );

                return false;
            }

            entity.TargetFrameworksText = project.TargetFrameworksText;
            entity.Authors = project.Authors;
            entity.Company = project.Company;
            entity.Copyright = project.Copyright;
            entity.Description = project.Description;
            entity.FileVersionText = project.FileVersionText;
            entity.PackageVersionText = project.PackageVersionText;
            entity.RootNamespace = project.RootNamespace;

            entity.Synchronized = true;

            return true;
        }

        #endregion

        #region Namespace

        public NamespaceDb? GetNamespace(INamespaceSymbol symbol, bool createIfMissing = false, bool updateExisting = false )
        {
            var fqn = symbol.ToUniqueName();

            var retVal = _dbContext.Namespaces
                .Include(x => x.SharpObject)
                .FirstOrDefault(x => x.SharpObject.FullyQualifiedName == fqn);

            if (retVal != null)
            {
                if (updateExisting)
                    UpdateNamespace(symbol, retVal);

                return retVal;
            }

            if (!createIfMissing)
            {
                _logger.Error<string>("Couldn't find AssemblyDb object for '{0}' in the database", fqn);
                return null;
            }

            retVal = new NamespaceDb
            {
                SharpObject = GetSharpObject(symbol, true, SharpObjectType.Namespace)!,
            };

            _dbContext.Namespaces.Add(retVal);

            UpdateNamespace(symbol, retVal);

            return retVal;
        }

        public bool UpdateNamespace(INamespaceSymbol symbol, NamespaceDb entity)
        {
            if (!SharpObjectMatchesSymbol(symbol, entity))
                return false;

            entity.SharpObject.Synchronized = true;

            return true;
        }

        #endregion

        public AssemblyNamespaceDb? GetAssemblyNamespace(
            AssemblyDb assemblyDb,
            NamespaceDb nsDb,
            bool createIfMissing = false)
        {
            var retVal = _dbContext.AssemblyNamespaces.FirstOrDefault(x =>
                x.AssemblyID == assemblyDb.SharpObjectID && x.NamespaceID == nsDb.SharpObjectID);

            if (retVal != null)
                return retVal;

            if (!createIfMissing)
            {
                // make sure we can access the fully qualified names
                _dbContext.Entry(assemblyDb)
                    .Reference(x => x.SharpObject)
                    .Load();

                _dbContext.Entry(nsDb)
                    .Reference(x => x.SharpObject)
                    .Load();

                _logger.Error<string, string>(
                    "Couldn't create AssemblyNamespaceDb for assembly '{0}' and namespace '{1}' ",
                    assemblyDb.SharpObject.FullyQualifiedName,
                    nsDb.SharpObject.FullyQualifiedName);

                return null;
            }

            retVal = new AssemblyNamespaceDb()
            {
                Assembly = assemblyDb,
                Namespace = nsDb
            };

            _dbContext.AssemblyNamespaces.Add(retVal);

            return retVal;
        }

        #region Types

        #region FixedType

        public FixedTypeDb? GetFixedType( INamedTypeSymbol symbol, bool createIfMissing = false,
            bool updateExisting = false )
        {
            var fqn = symbol.ToUniqueName();

            if( symbol.IsGenericType )
            {
                _logger.Error<string>( "Requested a FixedTypeDb from generic INamedTypeSymbol '{0}'", fqn );
                return null;
            }

            var retVal = _dbContext.FixedTypes
                .Include( x => x.SharpObject )
                .FirstOrDefault( x => x.SharpObject.FullyQualifiedName == fqn );

            if( retVal != null )
            {
                if( updateExisting )
                    UpdateFixedType( symbol, retVal );

                return retVal;
            }

            if( !createIfMissing )
            {
                _logger.Error<string>( "Couldn't find FixedTypeDb object for '{0}' in the database", fqn );
                return null;
            }

            if( !GetContainingAssemblyNamespace( symbol, out var assemblyDb, out var nsDb ) )
                return null;

            LoadInScopeInfo(assemblyDb!);

            retVal = new FixedTypeDb
            {
                SharpObject = GetSharpObject( symbol, true, SharpObjectType.FixedType )!,
                Assembly = assemblyDb!,
                Namespace = nsDb!,
                InDocumentationScope = assemblyDb!.InScopeInfo != null
            };

            _dbContext.FixedTypes.Add( retVal );

            UpdateFixedType(symbol, retVal);

            return retVal;
        }

        public bool UpdateFixedType(INamedTypeSymbol symbol, FixedTypeDb entity)
        {
            if (!SharpObjectMatchesSymbol(symbol, entity))
                return false;

            if (symbol.IsGenericType)
            {
                _logger.Error<string>( "Requested a FixedTypeDb from generic INamedTypeSymbol '{0}'",
                    symbol.ToUniqueName() );
                return false;
            }

            entity.DeclarationModifier = symbol.GetDeclarationModifier();
            entity.Accessibility = symbol.DeclaredAccessibility;
            entity.Nature = symbol.TypeKind;

            entity.SharpObject.Synchronized = true;

            return true;
        }

        #endregion

        #region GenericType

        public GenericTypeDb? GetGenericType(INamedTypeSymbol symbol, bool createIfMissing = false, bool updateExisting = false )
        {
            var fqn = symbol.ToUniqueName();

            if (!symbol.IsGenericType)
            {
                _logger.Error<string>("Requested a GenericTypeDb from a non-generic INamedTypeSymbol '{0}'", fqn);
                return null;
            }

            var retVal = _dbContext.GenericTypes
                .Include(x => x.SharpObject)
                .FirstOrDefault(x => x.SharpObject.FullyQualifiedName == fqn);

            if (retVal != null)
            {
                if (updateExisting)
                    UpdateGenericType(symbol, retVal);

                return retVal;
            }

            if (!createIfMissing)
            {
                _logger.Error<string>("Couldn't find FixedTypeDb object for '{0}' in the database", fqn);
                return null;
            }

            if (!GetContainingAssemblyNamespace(symbol, out var assemblyDb, out var nsDb))
                return null;

            LoadInScopeInfo( assemblyDb! );

            retVal = new GenericTypeDb
            {
                SharpObject = GetSharpObject(symbol, true, SharpObjectType.GenericType)!,
                Assembly = assemblyDb!,
                Namespace = nsDb!,
            };

            _dbContext.GenericTypes.Add(retVal);

            UpdateGenericType(symbol, retVal);

            return retVal;
        }

        public bool UpdateGenericType(INamedTypeSymbol symbol, GenericTypeDb entity)
        {
            if (!SharpObjectMatchesSymbol(symbol, entity))
                return false;

            if (!symbol.IsGenericType)
            {
                _logger.Error<string>("Requested a GenericTypeDb from a non-generic INamedTypeSymbol '{0}'",
                    symbol.ToUniqueName());
                return false;
            }

            entity.DeclarationModifier = symbol.GetDeclarationModifier();
            entity.Accessibility = symbol.DeclaredAccessibility;
            entity.Nature = symbol.TypeKind;

            entity.SharpObject.Synchronized = true;

            return true;
        }

#endregion

        public ImplementableTypeDb? GetImplementableType( INamedTypeSymbol symbol, bool createIfMissing = false, bool updateExisting = false )
        {
            if( symbol.IsGenericType )
                return GetGenericType( symbol, createIfMissing, updateExisting );

            return GetFixedType( symbol, createIfMissing, updateExisting );
        }

        public bool UpdateImplementableType(INamedTypeSymbol symbol, ImplementableTypeDb entity )
        {
            if( symbol.IsGenericType )
            {
                if( entity is GenericTypeDb genericEntity )
                    return UpdateGenericType( symbol, genericEntity );

                LoadSharpObject( entity );

                _logger.Error<string, string>("Trying to update FixedTypeDb entity '{0}' from generic INamedTypeSymbol '{1}'",
                    entity.SharpObject.FullyQualifiedName,
                    symbol.ToUniqueName());

                return false;
            }

            if( entity is FixedTypeDb fixedEntity )
                return UpdateFixedType( symbol, fixedEntity );

            LoadSharpObject(entity);

            _logger.Error<string, string>("Trying to update GenericTypeDb entity '{0}' from non-generic INamedTypeSymbol '{1}'",
                entity.SharpObject.FullyQualifiedName,
                symbol.ToUniqueName());

            return false;
        }

        #region ParametricType

        public ParametricTypeDb? GetParametricType( ITypeParameterSymbol symbol, bool createIfMissing = false, bool updateExisting = false )
        {
            var fqn = symbol.ToUniqueName();

            if( symbol.DeclaringType == null )
            {
                _logger.Error<string>(
                    "Requested a ParametericTypeDb from an ITypeParameterSymbol with an undefined DeclaringType '{0}'",
                    fqn );

                return null;
            }

            var retVal = _dbContext.ParametricTypes
                .Include( x => x.SharpObject )
                .FirstOrDefault( x => x.SharpObject.FullyQualifiedName == fqn );

            if (retVal != null)
            {
                if (updateExisting)
                    UpdateParametricType(symbol, retVal);

                return retVal;
            }

            if ( !createIfMissing )
            {
                _logger.Error<string>("Couldn't find ParametericTypeDb object for '{0}' in the database", fqn);
                return null;
            }

            if (!GetContainingAssemblyNamespace(symbol, out var assemblyDb, out var nsDb))
                return null;

            LoadInScopeInfo(assemblyDb!);

            retVal = new ParametricTypeDb
            {
                SharpObject = GetSharpObject(symbol, true, SharpObjectType.ParametricType)!,
                Assembly = assemblyDb!,
                Namespace = nsDb!,
                InDocumentationScope = assemblyDb!.InScopeInfo != null
            };

            _dbContext.ParametricTypes.Add( retVal );

            UpdateParametricType(symbol, retVal);

            return retVal;
        }

        public bool UpdateParametricType(ITypeParameterSymbol symbol, ParametricTypeDb entity )
        {
            if (!SharpObjectMatchesSymbol(symbol, entity))
                return false;

            if( symbol.DeclaringType == null )
            {
                _logger.Error<string>(
                    "Updating a ParametericTypeDb from an ITypeParameterSymbol with an undefined DeclaringType '{0}'",
                    symbol.ToUniqueName() );

                return false;
            }

            entity.Constraints = symbol.GetParametricTypeConstraint();
            entity.Accessibility = symbol.DeclaredAccessibility;
            entity.Nature = symbol.TypeKind;

            entity.SharpObject.Synchronized = true;

            return true;
        }

        #endregion

        #region ParametricMethodType

        public ParametricMethodTypeDb? GetParametricMethodType(ITypeParameterSymbol symbol, bool createIfMissing = false, bool updateExisting = false )
        {
            var fqn = symbol.ToUniqueName();

            if (symbol.DeclaringMethod == null)
            {
                _logger.Error<string>(
                    "Requested a ParametericMethodTypeDb from an ITypeParameterSymbol with an undefined DeclaringMethod '{0}'",
                    fqn);

                return null;
            }

            var retVal = _dbContext.ParametricMethodTypes
                .Include(x => x.SharpObject)
                .FirstOrDefault(x => x.SharpObject.FullyQualifiedName == fqn);

            if (retVal != null)
            {
                if (updateExisting)
                    UpdateParametricMethodType(symbol, retVal);

                return retVal;
            }

            if (!createIfMissing)
            {
                _logger.Error<string>("Couldn't find ParametericMethodTypeDb object for '{0}' in the database", fqn);
                return null;
            }

            if (!GetContainingAssemblyNamespace(symbol, out var assemblyDb, out var nsDb))
                return null;

            LoadInScopeInfo(assemblyDb!);

            retVal = new ParametricMethodTypeDb
            {
                SharpObject = GetSharpObject(symbol, true, SharpObjectType.ParametricMethodType)!,
                Assembly = assemblyDb!,
                Namespace = nsDb!,
                Constraints = symbol.GetParametricTypeConstraint(),
                InDocumentationScope = assemblyDb!.InScopeInfo != null
            };

            _dbContext.ParametricMethodTypes.Add(retVal);

            UpdateParametricMethodType(symbol, retVal);

            return retVal;
        }

        public bool UpdateParametricMethodType(ITypeParameterSymbol symbol, ParametricMethodTypeDb entity)
        {
            if (!SharpObjectMatchesSymbol(symbol, entity))
                return false;

            if (symbol.DeclaringMethod == null)
            {
                _logger.Error<string>(
                    "Updating a ParametericMethodTypeDb from an ITypeParameterSymbol with an undefined DeclaringMethod '{0}'",
                    symbol.ToUniqueName());

                return false;
            }

            entity.Constraints = symbol.GetParametricTypeConstraint();
            entity.Accessibility = symbol.DeclaredAccessibility;
            entity.Nature = symbol.TypeKind;

            entity.SharpObject.Synchronized = true;

            return true;
        }

        #endregion

        #region unspecified parametric

        public BaseTypeDb? GetUnspecifiedParametricType( ITypeParameterSymbol symbol, bool createIfMissing = false, bool updateExisting = false )
        {
            var fqn = symbol.ToUniqueName();

            if ( symbol.DeclaringType != null )
                return GetParametricType( symbol, createIfMissing, updateExisting );

            if( symbol.DeclaringMethod != null )
                return GetParametricMethodType( symbol, createIfMissing, updateExisting );

            _logger.Error<string>(
                "ITypeParameterSymbol '{0}' has neither a defined DeclaringMethod nor a defined DeclaringType",
                fqn);

            return null;
        }

        public bool UpdateUnspecifiedParametricType(ITypeParameterSymbol symbol, BaseTypeDb entity )
        {
            if( symbol.DeclaringType != null )
            {
                if( entity is ParametricTypeDb parametricEntity )
                    return UpdateParametricType( symbol, parametricEntity );

                LoadSharpObject(entity);

                _logger.Error<string, string>("Trying to update entity '{0}' from DeclaringType ITypeParameterSymbol '{1}'",
                    entity.SharpObject.FullyQualifiedName,
                    symbol.ToUniqueName());

                return false;
            }

            if( symbol.DeclaringMethod != null )
            {
                if (entity is ParametricMethodTypeDb parametricMethodEntity)
                    return UpdateParametricMethodType(symbol, parametricMethodEntity);

                LoadSharpObject(entity);

                _logger.Error<string, string>("Trying to update entity '{0}' from DeclaringMethod ITypeParameterSymbol '{1}'",
                    entity.SharpObject.FullyQualifiedName,
                    symbol.ToUniqueName());

                return false;
            }

            _logger.Error<string>(
                "ITypeParameterSymbol '{0}' has neither a defined DeclaringMethod nor a defined DeclaringType",
                symbol.ToUniqueName());

            return false;
        }

#endregion

        #region ArrayType

        public BaseTypeDb? GetArrayType( IArrayTypeSymbol symbol, bool createIfMissing = false, bool updateExisting = false )
        {
            var fqn = symbol.ToUniqueName();

            var retVal = _dbContext.ArrayTypes
                .Include(x => x.SharpObject)
                .FirstOrDefault(x => x.SharpObject.FullyQualifiedName == fqn);

            if (retVal != null)
            {
                if (updateExisting)
                    UpdateArrayType(symbol, retVal);

                return retVal;
            }

            if (!createIfMissing)
            {
                _logger.Error<string>("Couldn't find ArrayType object for '{0}' in the database", fqn);
                return null;
            }

            if (!GetContainingAssemblyNamespace(symbol, out var assemblyDb, out var nsDb))
                return null;

            LoadInScopeInfo(assemblyDb!);

            var elementDb = GetUnspecifiedType( symbol.ElementType );

            if( elementDb == null )
            {
                _logger.Error<string>( "Couldn't find ElementType '{0}' for ArrayType '{0}' in the database",
                    symbol.ElementType.ToFullName()
                    , fqn );

                return null;
            }

            retVal = new ArrayTypeDb
            {
                SharpObject = GetSharpObject(symbol, true, SharpObjectType.ArrayType)!,
                Assembly = assemblyDb!,
                Namespace = nsDb!,
                ElementType = elementDb,
                InDocumentationScope = assemblyDb!.InScopeInfo != null
            };

            _dbContext.ArrayTypes.Add(retVal);

            UpdateArrayType(symbol, retVal);

            return retVal;
        }

        public bool UpdateArrayType(IArrayTypeSymbol symbol, ArrayTypeDb entity )
        {
            if (!SharpObjectMatchesSymbol(symbol, entity))
                return false;

            entity.Accessibility = symbol.DeclaredAccessibility;
            entity.Nature = symbol.TypeKind;
            entity.Rank = symbol.Rank;

            entity.SharpObject.Synchronized = true;

            return true;
        }

        #endregion

        #region unspecified

        public BaseTypeDb? GetUnspecifiedType( ITypeSymbol symbol, bool createIfMissing = false, bool updateExisting = false )
        {
            var fqn = symbol.ToFullName();

            return symbol switch
            {
                INamedTypeSymbol ntSymbol => GetImplementableType( ntSymbol, createIfMissing, updateExisting ),
                ITypeParameterSymbol tpSymbol => GetUnspecifiedParametricType( tpSymbol, createIfMissing, updateExisting ),
                IArrayTypeSymbol arraySymbol => GetArrayType( arraySymbol, createIfMissing, updateExisting ),
                _ => unhandled()
            };

            BaseTypeDb? unhandled()
            {
                _logger.Error<string, Type>(
                    "ITypeSymbol '{0}' ({1}) is unsupported ",
                    fqn,
                    symbol.GetType() );

                return null;
            }
        }

        public bool UpdateUnspecifiedType( ITypeSymbol symbol, BaseTypeDb entity )
        {
            if( symbol is INamedTypeSymbol ntSymbol )
            {
                if( entity is ImplementableTypeDb implEntity )
                    return UpdateImplementableType( ntSymbol, implEntity );

                LoadSharpObject( entity );

                _logger.Error<string, string>( "Trying to update entity '{0}' from ITypeSymbol '{1}'",
                    entity.SharpObject.FullyQualifiedName,
                    symbol.ToFullName() );

                return false;
            }

            if( symbol is ITypeParameterSymbol tpSymbol )
            {
                if( entity is ParametricTypeDb || entity is ParametricMethodTypeDb )
                    return UpdateUnspecifiedParametricType( tpSymbol, entity );

                LoadSharpObject( entity );

                _logger.Error<string, string>( "Trying to update entity '{0}' from ITypeSymbol '{1}'",
                    entity.SharpObject.FullyQualifiedName,
                    symbol.ToFullName() );

                return false;
            }

            if( symbol is IArrayTypeSymbol arraySymbol )
            {
                if( entity is ArrayTypeDb arrayEntity )
                    return UpdateArrayType( arraySymbol, arrayEntity );

                LoadSharpObject(entity);

                _logger.Error<string, string>("Trying to update entity '{0}' from ITypeSymbol '{1}'",
                    entity.SharpObject.FullyQualifiedName,
                    symbol.ToFullName());

                return false;
            }

            _logger.Error<string>(
                "ITypeSymbol '{0}' is neither an INamedTypeSymbol, an ITypeParameterSymbol nor an IArrayTypeSymbol",
                symbol.ToFullName() );

            return false;
        }

        #endregion

        #region miscellaneous

        public TypeArgumentDb? GetTypeArgument(GenericTypeDb genericDb, ITypeSymbol symbol, int ordinal, bool createIfMissing = false)
        {
            var argDb = GetUnspecifiedType(symbol);

            if (argDb == null)
            {
                // make sure we can access the fully qualified names
                _dbContext.Entry(genericDb)
                    .Reference(x => x.SharpObject)
                    .Load();

                _logger.Error<string, string>(
                    "Couldn't find ParametricType for type '{0}' from symbol '{1}",
                    genericDb.SharpObject.FullyQualifiedName,
                    symbol.ToFullName());

                return null;
            }

            var retVal = _dbContext.TypeArguments
                .FirstOrDefault(ta => ta.ArgumentTypeID == genericDb!.SharpObjectID && ta.Ordinal == ordinal);

            if (retVal != null)
            {
                retVal.Synchronized = true;
                retVal.ArgumentType = argDb;

                return retVal;
            }

            if (!createIfMissing)
            {
                // make sure we can access the fully qualified names
                _dbContext.Entry(genericDb)
                    .Reference(x => x.SharpObject)
                    .Load();

                _logger.Error<string>(
                    "Couldn't create TypeArgument for type '{0}'",
                    genericDb.SharpObject.FullyQualifiedName);

                return null;
            }

            retVal = new TypeArgumentDb
            {
                DeclaringType = genericDb,
                ArgumentType = argDb,
                Ordinal = ordinal,
                Synchronized = true
            };

            return retVal;
        }

        public TypeAncestorDb? GetTypeAncestor(BaseTypeDb typeDb, ImplementableTypeDb ancestorDb, bool createIfMissing = false)
        {
            var retVal = _dbContext.TypeAncestors.FirstOrDefault(x =>
                x.AncestorTypeID == ancestorDb.SharpObjectID && x.ChildTypeID == typeDb.SharpObjectID);

            if (retVal != null)
                return retVal;

            if (!createIfMissing)
            {
                // make sure we can access the fully qualified name
                _dbContext.Entry(ancestorDb)
                    .Reference(x => x.SharpObject)
                    .Load();

                _logger.Error<string, string>(
                    "Couldn't create TypeAncestorDb for entity '{0}' and ancestor symbol '{1}' ",
                    typeDb.SharpObject.FullyQualifiedName,
                    ancestorDb.SharpObject.FullyQualifiedName);

                return null;
            }

            retVal = new TypeAncestorDb
            {
                AncestorTypeID = ancestorDb!.SharpObjectID,
                ChildTypeID = typeDb!.SharpObjectID
            };

            _dbContext.TypeAncestors.Add(retVal);

            return retVal;
        }

        #endregion

#endregion

        #region Method

        public MethodDb? GetMethod( IMethodSymbol symbol, bool createIfMissing = false, bool updateExisting = false )
        {
            var fqn = symbol.ToUniqueName();

            var retVal = _dbContext.Methods
                .Include(x => x.SharpObject)
                .FirstOrDefault(x => x.SharpObject.FullyQualifiedName == fqn);

            if (retVal != null)
            {
                if (updateExisting)
                    UpdateMethod(symbol, retVal);

                return retVal;
            }

            if (!createIfMissing)
            {
                _logger.Error<string>("Couldn't find MethodDb object for '{0}' in the database", fqn);
                return null;
            }

            if( !GetContainingAndReturnValueTypes( symbol, out var returnDb, out var containingDb ) )
                return null;

            retVal = new MethodDb
            {
                SharpObject = GetSharpObject(symbol, true, SharpObjectType.MethodArgument)!,
                DefiningType = containingDb!,
                ReturnType = returnDb!
            };

            _dbContext.Methods.Add(retVal);

            UpdateMethod(symbol, retVal);

            return retVal;
        }

        public bool UpdateMethod(IMethodSymbol symbol, MethodDb entity)
        {
            if (!SharpObjectMatchesSymbol(symbol, entity))
                return false;

            entity.Accessibility = symbol.DeclaredAccessibility;
            entity.DeclarationModifier = symbol.GetDeclarationModifier();
            entity.Kind = symbol.MethodKind;
            entity.ReturnsByRef = symbol.ReturnsByRef;
            entity.ReturnsByRefReadOnly = symbol.ReturnsByRefReadonly;
            entity.IsAbstract = symbol.IsAbstract;
            entity.IsExtern = symbol.IsExtern;
            entity.IsOverride = symbol.IsOverride;
            entity.IsReadOnly = symbol.IsReadOnly;
            entity.IsSealed = symbol.IsSealed;
            entity.IsStatic = symbol.IsStatic;
            entity.IsVirtual = symbol.IsVirtual;

            entity.SharpObject.Synchronized = true;

            return true;
        }

        public ArgumentDb? GetArgument(IParameterSymbol symbol, bool createIfMissing = false, bool updateExisting = false)
        {
            var fqn = symbol.ToUniqueName();

            if( !( symbol.ContainingSymbol is IMethodSymbol methodSymbol ) )
            {
                _logger.Error<string>("Trying to retrieve a method argument from IParameterSymbol '{0}' which is not contained by a method",
                    fqn);

                return null;
            }

            var retVal = _dbContext.MethodArguments
                .Include(x=>x.SharpObject  )
                .FirstOrDefault(x => x.SharpObject.FullyQualifiedName == fqn);

            if (retVal != null)
            {
                if (updateExisting)
                    UpdateArgument(symbol, retVal);

                return retVal;
            }

            if (!createIfMissing)
            {
                _logger.Error<string>("Couldn't find ArgumentDb object for '{0}' in the database", fqn);
                return null;
            }

            var methodDb = GetMethod( methodSymbol );

            if( methodDb == null )
            {
                _logger.Error<string>("Containing method '{0}' for IParameterSymbol '{1}' not found",
                    methodSymbol.ToUniqueName(),
                    fqn);

                return null;
            }

            var typeDb = GetUnspecifiedType( symbol.Type );

            if( typeDb == null )
            {
                _logger.Error<string>("Type ({0}) for IParameterSymbol '{1}' not found",
                    symbol.Type.GetUniqueName(),
                    fqn);

                return null;
            }

            retVal = new ArgumentDb
            {
                SharpObject = GetSharpObject(symbol, true, SharpObjectType.MethodArgument)!,
                Method = methodDb,
                ArgumentType = typeDb
            };

            _dbContext.MethodArguments.Add(retVal);

            UpdateArgument(symbol, retVal);

            return retVal;
        }

        public bool UpdateArgument( IParameterSymbol symbol, ArgumentDb entity )
        {
            if( !SharpObjectMatchesSymbol( symbol, entity ) )
                return false;

            if( !( symbol.ContainingSymbol is IMethodSymbol methodSymbol ) )
            {
                _logger.Error<string>(
                    "Trying to update a method argument from IParameterSymbol '{0}' which is not contained by a method",
                    symbol.ToUniqueName() );

                return false;
            }

            entity.Ordinal = symbol.Ordinal;
            entity.IsOptional = symbol.IsOptional;
            entity.IsParams = symbol.IsParams;
            entity.IsThis = symbol.IsThis;
            entity.IsDiscard = symbol.IsDiscard;
            entity.ReferenceKind = symbol.RefKind;
            entity.DefaultText = "need to determine default text";

            entity.SharpObject.Synchronized = true;

            return true;
        }

        #endregion

        #region Property

        public PropertyDb? GetProperty( IPropertySymbol symbol, bool createIfMissing = false, bool updateExisting = false )
        {
            var fqn = symbol.ToUniqueName();

            var retVal = _dbContext.Properties
                .Include( x => x.SharpObject )
                .FirstOrDefault( x => x.SharpObject.FullyQualifiedName == fqn );

            if (retVal != null)
            {
                if (updateExisting)
                    UpdateProperty(symbol, retVal);

                return retVal;
            }

            if ( !createIfMissing )
            {
                _logger.Error<string>( "Couldn't find PropertyDb object for '{0}' in the database", fqn );
                return null;
            }

            if( !GetContainingAndReturnValueTypes( symbol, out var propDb, out var containingDb ) )
                return null;

            retVal = new PropertyDb
            {
                SharpObject = GetSharpObject( symbol, true, SharpObjectType.Property )!,
                DefiningType = containingDb!,
                PropertyType = propDb!
            };

            _dbContext.Properties.Add( retVal );

            UpdateProperty(symbol, retVal);

            return retVal;
        }

        public bool UpdateProperty(IPropertySymbol symbol, PropertyDb entity)
        {
            if (!SharpObjectMatchesSymbol(symbol, entity))
                return false;

            entity.GetAccessibility = symbol.GetMethod?.DeclaredAccessibility ?? Accessibility.NotApplicable;
            entity.SetAccessibility = symbol.SetMethod?.DeclaredAccessibility ?? Accessibility.NotApplicable;
            entity.DeclarationModifier = symbol.GetDeclarationModifier();
            entity.ReturnsByRef = symbol.ReturnsByRef;
            entity.ReturnsByRefReadOnly = symbol.ReturnsByRefReadonly;
            entity.IsAbstract = symbol.IsAbstract;
            entity.IsExtern = symbol.IsExtern;
            entity.IsIndexer = symbol.IsIndexer;
            entity.IsOverride = symbol.IsOverride;
            entity.IsReadOnly = symbol.IsReadOnly;
            entity.IsSealed = symbol.IsSealed;
            entity.IsStatic = symbol.IsStatic;
            entity.IsVirtual = symbol.IsVirtual;
            entity.IsWriteOnly = symbol.IsWriteOnly;

            entity.SharpObject.Synchronized = true;

            return true;
        }

        public PropertyParameterDb? GetPropertyParameter(IParameterSymbol symbol, bool createIfMissing = false, bool updateExisting = false)
        {
            var fqn = symbol.ToUniqueName();

            if (!(symbol.ContainingSymbol is IPropertySymbol propSymbol))
            {
                _logger.Error<string>("Trying to retrieve a property parameter from IParameterSymbol '{0}' which is not contained by a property",
                    fqn);

                return null;
            }

            var retVal = _dbContext.PropertyParameters
                .Include(x => x.SharpObject)
                .FirstOrDefault(x => x.SharpObject.FullyQualifiedName == fqn);

            if (retVal != null)
            {
                if (updateExisting)
                    UpdatePropertyParameter(symbol, retVal);

                return retVal;
            }

            if (!createIfMissing)
            {
                _logger.Error<string>("Couldn't find PropertyParameterDb object for '{0}' in the database", fqn);
                return null;
            }

            var propDb = GetProperty(propSymbol);

            if (propDb == null)
            {
                _logger.Error<string>("Containing method '{0}' for IParameterSymbol '{1}' not found",
                    propSymbol.ToUniqueName(),
                    fqn);

                return null;
            }

            if( !( symbol.Type is INamedTypeSymbol ntSymbol ) )
            {
                _logger.Error<string>("Type '{0}' of IParameterSymbol '{1}' is not an INamedTypeSymbol",
                    symbol.Type.ToFullName(),
                    fqn);

                return null;
            }

            var typeDb = GetImplementableType( ntSymbol );

            if (typeDb == null)
            {
                _logger.Error<string>("Type ({0}) for IParameterSymbol '{1}' not found",
                    ntSymbol.ToUniqueName(),
                    fqn);

                return null;
            }

            retVal = new PropertyParameterDb
            {
                SharpObject = GetSharpObject(symbol, true, SharpObjectType.PropertyParameter)!,
                Property = propDb,
                ParameterType = typeDb
            };

            _dbContext.PropertyParameters.Add(retVal);

            UpdatePropertyParameter(symbol, retVal);

            return retVal;
        }

        public bool UpdatePropertyParameter( IParameterSymbol symbol, PropertyParameterDb entity )
        {
            if( !SharpObjectMatchesSymbol( symbol, entity ) )
                return false;

            if( !( symbol.ContainingSymbol is IPropertySymbol propSymbol ) )
            {
                _logger.Error<string>(
                    "Trying to update a property parameter from IParameterSymbol '{0}' which is not contained by a property",
                    symbol.ToUniqueName() );

                return false;
            }

            entity.Ordinal = symbol.Ordinal;
            entity.IsAbstract = symbol.IsAbstract;
            entity.IsExtern = symbol.IsExtern;
            entity.IsOverride = symbol.IsOverride;
            entity.IsSealed = symbol.IsSealed;
            entity.IsStatic = symbol.IsStatic;
            entity.IsVirtual = symbol.IsVirtual;

            entity.SharpObject.Synchronized = true;

            return true;
        }

        #endregion

        #region Field

        public FieldDb? GetField(IFieldSymbol symbol, bool createIfMissing = false, bool updateExisting = false)
        {
            var fqn = symbol.ToUniqueName();

            var retVal = _dbContext.Fields
                .Include(x => x.SharpObject)
                .FirstOrDefault(x => x.SharpObject.FullyQualifiedName == fqn);

            if (retVal != null)
            {
                if (updateExisting)
                    UpdateField(symbol, retVal);

                return retVal;
            }

            if (!createIfMissing)
            {
                _logger.Error<string>("Couldn't find FieldDb object for '{0}' in the database", fqn);
                return null;
            }

            if (!GetContainingAndReturnValueTypes(symbol, out var propDb, out var containingDb))
                return null;

            retVal = new FieldDb
            {
                SharpObject = GetSharpObject(symbol, true, SharpObjectType.Field)!,
                DefiningType = containingDb!,
                FieldType = propDb!
            };

            _dbContext.Fields.Add(retVal);

            UpdateField(symbol, retVal);

            return retVal;
        }

        public bool UpdateField(IFieldSymbol symbol, FieldDb entity)
        {
            if (!SharpObjectMatchesSymbol(symbol, entity))
                return false;

            entity.Accessibility = symbol.DeclaredAccessibility;
            entity.DeclarationModifier = symbol.GetDeclarationModifier();
            entity.IsAbstract = symbol.IsAbstract;
            entity.IsExtern = symbol.IsExtern;
            entity.IsOverride = symbol.IsOverride;
            entity.IsReadOnly = symbol.IsReadOnly;
            entity.IsSealed = symbol.IsSealed;
            entity.IsStatic = symbol.IsStatic;
            entity.IsVirtual = symbol.IsVirtual;

            entity.SharpObject.Synchronized = true;

            return true;
        }

#endregion

        private SharpObject? GetSharpObject(ISymbol symbol, bool createIfMissing = false, SharpObjectType soType = SharpObjectType.Unknown )
        {
            var fqn = symbol.GetUniqueName();

            var retVal = _dbContext.SharpObjects.FirstOrDefault(x => x.FullyQualifiedName == fqn);

            if (retVal != null)
                return retVal;

            if (!createIfMissing)
            {
                _logger.Error<string>("Couldn't find SharpObject for '{0}' in the database", fqn);
                return null;
            }

            if( soType == SharpObjectType.Unknown )
                throw new ArgumentException($"Can't create a SharpObject whose SharpObjectType is SharpObjectType.Unknown");

            retVal = new SharpObject
            {
                FullyQualifiedName = fqn!,
                Name = symbol.ToSimpleName(),
                Synchronized = true,
                SharpObjectType = soType
            };

            _dbContext.SharpObjects.Add(retVal);

            return retVal;
        }

        private void LoadSharpObject<TEntity>(TEntity entity)
            where TEntity : class, ISharpObject
        {
            _dbContext.Entry(entity)
                .Reference(x => x.SharpObject)
                .Load();
        }

        private void LoadInScopeInfo(AssemblyDb entity)
        {
            _dbContext.Entry(entity)
                .Reference(x => x.InScopeInfo)
                .Load();
        }

        private bool SharpObjectMatchesSymbol<TEntity>( ISymbol symbol, TEntity entity )
            where TEntity : class, ISharpObject
        {
            LoadSharpObject( entity );

            var fqn = symbol.GetUniqueName();

            if( entity.SharpObject.FullyQualifiedName.Equals( fqn, StringComparison.Ordinal ) )
                return true;

            _logger.Error<string, Type, string>( "ISymbol '{0}' does not correspond to {1} '{2}'",
                fqn,
                typeof(TEntity),
                entity.SharpObject.FullyQualifiedName );

            return false;
        }

        private bool GetContainingAssembly(ITypeSymbol symbol, out AssemblyDb? assemblyDb)
        {
            assemblyDb = symbol switch
            {
                IArrayTypeSymbol arraySymbol => GetAssembly(arraySymbol.ElementType.ContainingAssembly),
                _ => GetAssembly(symbol.ContainingAssembly)
            };

            if (assemblyDb == null)
            {
                _logger.Error<string, string>(
                    "Couldn't find AssemblyDb entity for ContainingAssembly '{0}' in ITypeSymbol '{1}'",
                    symbol.ContainingAssembly.ToUniqueName(),
                    symbol.GetUniqueName());

                return false;
            }

            return true;
        }

        private bool GetContainingAssemblyNamespace(ITypeSymbol symbol, out AssemblyDb? assemblyDb, out NamespaceDb? nsDb)
        {
            nsDb = null;

            assemblyDb = symbol switch
            {
                IArrayTypeSymbol arraySymbol => GetAssembly( arraySymbol.ElementType.ContainingAssembly ),
                _ => GetAssembly( symbol.ContainingAssembly )
            };

            if (assemblyDb == null)
            {
                _logger.Error<string, string>(
                    "Couldn't find AssemblyDb entity for ContainingAssembly '{0}' in ITypeSymbol '{1}'",
                    symbol.ContainingAssembly.ToUniqueName(),
                    symbol.GetUniqueName());

                return false;
            }

            nsDb = symbol switch
            {
                IArrayTypeSymbol arraySymbol => GetNamespace( arraySymbol.ElementType.ContainingNamespace ),
                _ => GetNamespace( symbol.ContainingNamespace )
            };

            if (nsDb == null)
            {
                _logger.Error<string, string>(
                    "Couldn't find NamespaceDb entity for ContainingNamespace '{0}' in ITypeSymbol '{1}'",
                    symbol.ContainingNamespace.ToUniqueName(),
                    symbol.GetUniqueName());

                return false;
            }

            // load the associated InScopeInfo
            _dbContext.Entry( assemblyDb )
                .Reference( x => x.InScopeInfo )
                .Load();

            return true;
        }

        private bool GetContainingAndReturnValueTypes(ISymbol symbol, out BaseTypeDb? returnDb, out ImplementableTypeDb? containingDb )
        {
            returnDb = null;

            containingDb = symbol switch
            {
                IMethodSymbol methodSymbol => GetImplementableType( methodSymbol.ContainingType ),
                IPropertySymbol propSymbol => GetImplementableType( propSymbol.ContainingType ),
                _ => null
            };

            if( containingDb == null )
            {
                _logger.Error<string, string>(
                    "Couldn't find entity for ContainingType '{0}' in INamedTypeSymbol '{1}'",
                    symbol.ContainingType.ToUniqueName(),
                    symbol.GetUniqueName());

                return false;
            }

            returnDb = symbol switch
            {
                IMethodSymbol methodSymbol => GetUnspecifiedType( methodSymbol.ReturnType ),
                IPropertySymbol propSymbol => GetUnspecifiedType( propSymbol.Type ),
                _ => null
            };

            if (returnDb == null)
            {
                _logger.Error<string, string>(
                    "Couldn't find entity for return value/property type '{0}' in INamedTypeSymbol '{1}'",
                    symbol.ContainingType.ToUniqueName(),
                    symbol.GetUniqueName());

                return false;
            }

            return true;
        }
    }
}