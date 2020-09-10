using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace J4JSoftware.Roslyn
{
    public class EntityFactories : IEntityFactories
    {
        public SymbolDisplayFormat UniqueNameFormat { get; } = SymbolDisplayFormat.FullyQualifiedFormat
            .WithGlobalNamespaceStyle( SymbolDisplayGlobalNamespaceStyle.Omitted )
            .WithGenericsOptions( SymbolDisplayGenericsOptions.IncludeTypeParameters )
            .WithMemberOptions( SymbolDisplayMemberOptions.IncludeContainingType
                                | SymbolDisplayMemberOptions.IncludeExplicitInterface )
            //| SymbolDisplayMemberOptions.IncludeParameters )
            .WithParameterOptions( SymbolDisplayParameterOptions.None )
            //.WithParameterOptions(SymbolDisplayParameterOptions.IncludeExtensionThis
            //                      | SymbolDisplayParameterOptions.IncludeName
            //                      | SymbolDisplayParameterOptions.IncludeParamsRefOut
            //                      | SymbolDisplayParameterOptions.IncludeDefaultValue
            //                      | SymbolDisplayParameterOptions.IncludeOptionalBrackets
            //                      | SymbolDisplayParameterOptions.IncludeType)
            .RemoveMiscellaneousOptions( SymbolDisplayMiscellaneousOptions.UseSpecialTypes );

        public SymbolDisplayFormat FullNameFormat { get; } = SymbolDisplayFormat.FullyQualifiedFormat
            .WithGlobalNamespaceStyle( SymbolDisplayGlobalNamespaceStyle.Omitted )
            .WithGenericsOptions( SymbolDisplayGenericsOptions.IncludeTypeParameters )
            .WithMemberOptions( SymbolDisplayMemberOptions.IncludeContainingType
                                | SymbolDisplayMemberOptions.IncludeExplicitInterface
                                | SymbolDisplayMemberOptions.IncludeParameters )
            .WithParameterOptions( SymbolDisplayParameterOptions.IncludeExtensionThis
                                   | SymbolDisplayParameterOptions.IncludeName
                                   | SymbolDisplayParameterOptions.IncludeParamsRefOut
                                   | SymbolDisplayParameterOptions.IncludeDefaultValue
                                   | SymbolDisplayParameterOptions.IncludeOptionalBrackets
                                   | SymbolDisplayParameterOptions.IncludeType )
            .RemoveMiscellaneousOptions( SymbolDisplayMiscellaneousOptions.UseSpecialTypes );

        public SymbolDisplayFormat GenericTypeFormat { get; } = SymbolDisplayFormat.FullyQualifiedFormat
            .WithGlobalNamespaceStyle( SymbolDisplayGlobalNamespaceStyle.Omitted )
            .RemoveMiscellaneousOptions( SymbolDisplayMiscellaneousOptions.UseSpecialTypes )
            .RemoveGenericsOptions( SymbolDisplayGenericsOptions.IncludeTypeParameters );

        public SymbolDisplayFormat SimpleNameFormat { get; } = SymbolDisplayFormat.MinimallyQualifiedFormat;

        private readonly List<IEntityFactory> _factories;
        private readonly IJ4JLogger _logger;

        public EntityFactories(
            RoslynDbContext dbContext,
            IEnumerable<IEntityFactory> factories,
            IJ4JLogger logger
        )
        {
            DbContext = dbContext;

            _factories = factories.ToList();
            _factories.ForEach( x => x.Factories = this );

            _logger = logger;
            _logger.SetLoggedType( this.GetType() );

            if( !_factories.Any() )
                _logger.Error( "No entity factories defined" );
        }

        #region Methods for getting symbol names

        public string GetFullName( ISymbol? symbol )
        {
            if( symbol == null )
                return "***undefined symbol***";

            if( GetUniqueName( symbol, out var fqn ) )
                return fqn;

            return symbol.ToDisplayString( FullNameFormat );
        }

        public bool GetUniqueName( ISymbol? symbol, out string result )
        {
            if( symbol == null )
            {
                result = string.Empty;
                _logger.Error( "symbol is undefined" );

                return false;
            }

            result = symbol.ToDisplayString( UniqueNameFormat );

            if( GetSharpObjectType( symbol ) == SharpObjectType.Unknown )
            {
                _logger.Error<string>( "Unhandled ISymbol '{0}'", result );

                return false;
            }

            switch( symbol )
            {
                case IAssemblySymbol aSymbol:
                case INamespaceSymbol nsSymbol:
                case IFieldSymbol fieldSymbol:
                    return true;

                case INamedTypeSymbol ntSymbol:
                    if( !GetFQN( ntSymbol, out var ntTemp ) )
                        return false;

                    result = ntTemp;

                    return true;

                case ITypeParameterSymbol tpSymbol:
                    if( !GetFQN( tpSymbol, out var tpTemp ) )
                        return false;

                    result = tpTemp;

                    return true;

                case IArrayTypeSymbol arraySymbol:
                    if( !GetFQN( arraySymbol, out var arrayTemp ) )
                        return false;

                    result = arrayTemp;

                    return true;

                case IMethodSymbol methodSymbol:
                    if( !GetFQN( methodSymbol, out var methodTemp ) )
                        return false;

                    result = methodTemp;

                    return true;

                case IPropertySymbol propSymbol:
                    if( !GetFQN( propSymbol, out var propTemp ) )
                        return false;

                    result = propTemp;

                    return true;
            }

            _logger.Error<string>( "Unhandled ISymbol '{0}'", result );

            return false;
        }

        public string GetName( ISymbol symbol )
        {
            return symbol switch
            {
                ITypeParameterSymbol tpSymbol => tpSymbol.TypeParameterKind switch
                {
                    TypeParameterKind.Method => tpSymbol.DeclaringMethod == null
                        ? string.Empty
                        : $"{tpSymbol.DeclaringMethod.ToDisplayString( SimpleNameFormat )}::{tpSymbol.Name}",
                    TypeParameterKind.Type => tpSymbol.DeclaringType == null
                        ? string.Empty
                        : $"{tpSymbol.DeclaringType.ToDisplayString( SimpleNameFormat )}::{tpSymbol.Name}",
                    _ => string.Empty
                },
                _ => symbol.ToDisplayString( SimpleNameFormat )
            };
        }

        #region internal name routines

        private bool GetFQN( INamedTypeSymbol symbol, out string result )
        {
            result = symbol.ToDisplayString( UniqueNameFormat );

            // non-generic types are simple...
            if( !symbol.IsGenericType )
                return true;

            var sb = new StringBuilder( symbol.ToDisplayString( GenericTypeFormat ) );

            sb.Append( "<" );

            // we identify type parameters by their ID from the database
            var allOkay = true;

            for( var argIdx = 0; argIdx < symbol.TypeArguments.Length; argIdx++ )
            {
                var argSymbol = symbol.TypeArguments[ argIdx ];

                if( argIdx > 0 )
                    sb.Append( ", " );

                if( Retrieve<TypeDb>( argSymbol, out var argDb ) )
                    sb.Append( argDb!.SharpObjectID );
                else
                {
                    sb.Append( 0 );

                    _logger.Error<string, string>(
                        "Couldn't find type entity for type parameter '{0}' on symbol '{1}'",
                        argSymbol.Name,
                        symbol.ToDisplayString( UniqueNameFormat ) );

                    allOkay = false;
                }
            }

            sb.Append( ">" );

            result = sb.ToString();

            return allOkay;
        }

        private bool GetFQN( ITypeParameterSymbol symbol, out string result )
        {
            result = string.Empty;

            if( symbol.DeclaringType != null )
                result = $"{symbol.DeclaringType.ToDisplayString( UniqueNameFormat )}::{symbol.Name}";

            if( symbol.DeclaringMethod != null )
                result = $"{symbol.DeclaringMethod.ToDisplayString( UniqueNameFormat )}::{symbol.Name}";

            _logger.Error<string>(
                "ITypeParameterSymbol '{0}' is contained neither by an IMethodSymbol nor an INamedTypeSymbol",
                symbol.ToDisplayString( UniqueNameFormat ) );

            return result != string.Empty;
        }

        private bool GetFQN( IArrayTypeSymbol symbol, out string result )
        {
            result = symbol.ToDisplayString( UniqueNameFormat );

            var retVal = true;

            switch( symbol.ElementType )
            {
                case INamedTypeSymbol ntSymbol:
                    if( !GetFQN( ntSymbol, out var temp1 ) )
                        return false;

                    result = temp1;

                    break;

                case ITypeParameterSymbol tpSymbol:
                    if( !GetFQN( tpSymbol, out var temp2 ) )
                        return false;

                    result = temp2;

                    break;

                default:
                    result = symbol.ToDisplayString( UniqueNameFormat );
                    retVal = false;
                    break;
            }

            result = $"{result}[{symbol.Rank}]";

            return retVal;
        }

        private bool GetFQN( IMethodSymbol symbol, out string result )
        {
            // get the method name without the paranetheses -- we'll add them
            // as we add the arguments
            var sb = new StringBuilder( symbol
                .ToDisplayString( UniqueNameFormat )
                .Replace( "()", string.Empty ) );

            sb.Append( symbol.Parameters.Length == 0 ? "(" : "( " );

            var allOkay = AddParametersToFQN( symbol, sb, symbol.Parameters );

            sb.Append( symbol.Parameters.Length == 0 ? ")" : " )" );

            result = sb.ToString();

            return allOkay;
        }

        private bool GetFQN( IPropertySymbol symbol, out string result )
        {
            // get the property name without any brackets -- we'll add them
            // as we add the parameters
            var sb = new StringBuilder( symbol
                .ToDisplayString( UniqueNameFormat )
                .Replace( "[]", string.Empty ) );

            if( symbol.Parameters.Length > 0 )
                sb.Append( "[   " );

            var allOkay = AddParametersToFQN( symbol, sb, symbol.Parameters );

            if( symbol.Parameters.Length > 0 )
                sb.Append( " ]" );

            result = sb.ToString();

            return allOkay;
        }

        private bool AddParametersToFQN( ISymbol parentSymbol, StringBuilder sb,
            ImmutableArray<IParameterSymbol> parameters )
        {
            var retVal = true;

            for( var argIdx = 0; argIdx < parameters.Length; argIdx++ )
            {
                if( argIdx > 0 )
                    sb.Append( ", " );

                var argSymbol = parameters[ argIdx ];

                // we identify each parameter's type by its type in the 
                // database
                if( Retrieve<TypeDb>( argSymbol, out var argDb ) )
                    sb.Append( $"{argDb!.SharpObjectID} {argSymbol.Name}" );
                else
                {
                    sb.Append( $"0 {argSymbol.Name}" );

                    _logger.Error<string, string>( "Couldn't find type for parameter '{0}' in method '{1}",
                        argSymbol.Name,
                        parentSymbol.ToDisplayString( UniqueNameFormat ) );

                    retVal = false;
                }
            }

            return retVal;
        }

        #endregion

        #endregion

        #region Methods for determining SharpObjectTypes

        public SharpObjectType GetSharpObjectType( ISymbol symbol )
        {
            return symbol switch
            {
                IAssemblySymbol aSymbol => SharpObjectType.Assembly,
                INamespaceSymbol nsSymbol => SharpObjectType.Namespace,
                INamedTypeSymbol ntSymbol => ntSymbol.IsGenericType
                    ? SharpObjectType.GenericType
                    : SharpObjectType.FixedType,
                IMethodSymbol mSymbol => SharpObjectType.Method,
                ITypeParameterSymbol tpSymbol => SharpObjectType.ParametricType,
                IPropertySymbol pSymbol => SharpObjectType.Property,
                IArrayTypeSymbol arraySymbol => SharpObjectType.ArrayType,
                IFieldSymbol fieldSymbol => SharpObjectType.Field,
                _ => SharpObjectType.Unknown
            };
        }

        public SharpObjectType GetSharpObjectType<TEntity>()
            where TEntity : ISharpObject
        {
            var entityType = typeof(TEntity);

            if( typeof(AssemblyDb) == entityType )
                return SharpObjectType.Assembly;

            if( typeof(NamespaceDb) == entityType )
                return SharpObjectType.Namespace;

            if( typeof(FixedTypeDb) == entityType )
                return SharpObjectType.FixedType;

            if( typeof(GenericTypeDb) == entityType )
                return SharpObjectType.GenericType;

            if( typeof(MethodDb) == entityType )
                return SharpObjectType.Method;

            if( typeof(ParametricTypeDb) == entityType )
                return SharpObjectType.ParametricType;

            if( typeof(TypeParametricTypeDb) == entityType )
                return SharpObjectType.ParametricType;

            if( typeof(MethodParametricTypeDb) == entityType )
                return SharpObjectType.ParametricType;

            if( typeof(PropertyDb) == entityType )
                return SharpObjectType.Property;

            return SharpObjectType.Unknown;
        }

        #endregion

        public RoslynDbContext DbContext { get; }

        public bool CanProcess<TEntity>( ISymbol symbol, bool createIfMissing )
            where TEntity : class, ISharpObject
        {
            var entityType = typeof(TEntity);

            // if we're not creating an entity a factory which will retrieve an
            // entity that derives from the target TEntity is okay
            return _factories.Any( f =>
                ( createIfMissing
                    ? f.EntityType == entityType
                    : f.EntityType == entityType || entityType.IsAssignableFrom( f.EntityType ) )
                && f.CanProcess( symbol ) );
        }

        public bool Retrieve<TEntity>( ISymbol? symbol, out TEntity? result, bool createIfMissing = false )
            where TEntity : class, ISharpObject
        {
            result = null;

            if( symbol == null )
                return false;

            var entityType = typeof(TEntity);

            // if we're not creating an entity a factory which will retrieve an
            // entity that derives from the target TEntity is okay
            var factory = _factories.FirstOrDefault( f =>
                ( createIfMissing
                    ? f.EntityType == entityType
                    : f.EntityType == entityType || entityType.IsAssignableFrom( f.EntityType ) )
                && f.CanProcess( symbol ) );

            if( factory == null )
            {
                _logger.Error(
                    "Couldn't find a factory which retrieves {0} entities and can process the provided ISymbol {1}",
                    entityType,
                    symbol.GetType() );

                return false;
            }

            if( !factory.Retrieve( symbol, out var innerResult, createIfMissing ) )
                return false;

            result = (TEntity) innerResult!;

            return true;
        }

        public bool RetrieveSharpObject( ISymbol symbol, out SharpObject? result, bool createIfMissing = false )
        {
            result = null;

            if( !GetUniqueName( symbol, out var fqn ) )
            {
                _logger.Error<string>( "Couldn't generate unique name of '{0}'",
                    symbol.ToDisplayString( UniqueNameFormat ) );

                return false;
            }

            var type = GetSharpObjectType( symbol );

            if( type == SharpObjectType.Unknown )
            {
                _logger.Error<string>( "Unknown SharpObjectType '{0}'", fqn );
                return false;
            }

            result = DbContext.SharpObjects.FirstOrDefault( x => x.FullyQualifiedName == fqn );

            if( result == null && createIfMissing )
            {
                result = new SharpObject { FullyQualifiedName = fqn };

                DbContext.SharpObjects.Add( result );
            }

            if( result == null )
                return false;

            result.Name = GetName( symbol );
            result.SharpObjectType = type;
            result.Synchronized = true;

            return true;
        }

        #region Methods for marking objects as synchronized or unsynchronized

        public void MarkSharpObjectUnsynchronized<TEntity>( bool saveChanges = false )
            where TEntity : class, ISharpObject
        {
            var entityType = typeof(TEntity);

            // update the underlying DocObject
            var docObjType = GetSharpObjectType<TEntity>();

            DbContext.SharpObjects.Where( x => x.SharpObjectType == docObjType )
                .ForEachAsync( x => x.Synchronized = false );

            if( saveChanges )
                DbContext.SaveChanges();
        }

        public void MarkUnsynchronized<TEntity>( bool saveChanges = false )
            where TEntity : class, ISynchronized
        {
            var entityType = typeof(TEntity);

            // update the entities directly
            var dbSet = DbContext.Set<TEntity>().Cast<ISynchronized>();

            dbSet.ForEachAsync( x => x.Synchronized = false );

            if( saveChanges )
                DbContext.SaveChanges();
        }

        public void MarkSynchronized<TEntity>( TEntity entity )
            where TEntity : class, ISharpObject
        {
            // we can't mark newly-created ISharpObjects as synchronized
            // ...so they must be marked that way when they're created
            var entry = DbContext.Entry( entity );

            switch( entry.State )
            {
                case EntityState.Added:
                case EntityState.Detached:
                    return;
            }

            entry.Reference( x => x.SharpObject )
                .Load();

            entity.SharpObject.Synchronized = true;
        }

        #endregion
    }
}