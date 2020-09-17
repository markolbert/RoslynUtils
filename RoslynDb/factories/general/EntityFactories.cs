using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Reflection;
using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace J4JSoftware.Roslyn
{
    public class EntityFactories
    {
        #region Format specifications

        public static SymbolDisplayFormat UniqueNameFormat { get; } = SymbolDisplayFormat.FullyQualifiedFormat
            .WithGlobalNamespaceStyle( SymbolDisplayGlobalNamespaceStyle.Omitted )
            .WithGenericsOptions( SymbolDisplayGenericsOptions.IncludeTypeParameters )
            .WithMemberOptions( SymbolDisplayMemberOptions.IncludeContainingType
                                | SymbolDisplayMemberOptions.IncludeExplicitInterface )
            .WithParameterOptions( SymbolDisplayParameterOptions.None )
            .RemoveMiscellaneousOptions( SymbolDisplayMiscellaneousOptions.UseSpecialTypes );

        public static SymbolDisplayFormat FullNameFormat { get; } = SymbolDisplayFormat.FullyQualifiedFormat
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

        public static SymbolDisplayFormat GenericTypeFormat { get; } = SymbolDisplayFormat.FullyQualifiedFormat
            .WithGlobalNamespaceStyle( SymbolDisplayGlobalNamespaceStyle.Omitted )
            .RemoveMiscellaneousOptions( SymbolDisplayMiscellaneousOptions.UseSpecialTypes )
            .RemoveGenericsOptions( SymbolDisplayGenericsOptions.IncludeTypeParameters );

#endregion

        public SymbolDisplayFormat SimpleNameFormat { get; } = SymbolDisplayFormat.MinimallyQualifiedFormat;

        private readonly List<IEntityFactory> _factories;
        private readonly Dictionary<Type, MethodInfo> _supportedSymbols = new Dictionary<Type, MethodInfo>();
        private readonly Dictionary<Type, SharpObjectType> _entityTypes = new Dictionary<Type, SharpObjectType>();

        private readonly IJ4JLogger _logger;

        public EntityFactories(
            RoslynDbContext dbContext,
            IEnumerable<IEntityFactory> factories,
            IJ4JLogger logger
        )
        {
            DbContext = dbContext;

            _logger = logger;
            _logger.SetLoggedType(this.GetType());

            _factories = factories.ToList();

            if (!_factories.Any())
                _logger.Error("No entity factories defined");

            // initialize the entity factories so they can refer to us
            foreach( var factory in _factories.Cast<IEntityFactoryInternal>() )
            {
                factory.SetFactories( this );
            }
        }

        #region Methods for getting symbol names

        public string GetFullName( ISymbol? symbol )
        {
            if( symbol == null )
                return "***undefined symbol***";

            if ( symbol is ITypeParameterSymbol tpSymbol )
            {
                ISymbol? declaringSymbol = null;

                if( tpSymbol.DeclaringType != null )
                    declaringSymbol = tpSymbol.DeclaringType;

                if( tpSymbol.DeclaringMethod != null )
                    declaringSymbol = tpSymbol.DeclaringMethod;

                return declaringSymbol != null 
                    ? $"{declaringSymbol.ToDisplayString( FullNameFormat )}:{tpSymbol.ToDisplayString( FullNameFormat )}" 
                    : tpSymbol.ToDisplayString( FullNameFormat );
            }

            return symbol.ToDisplayString( FullNameFormat );
        }

        public bool GetUniqueName( ISymbol? symbol, out string result, bool suppressMessages = false )
        {
            if( symbol == null )
            {
                result = string.Empty;

                if( !suppressMessages )
                    _logger.Error( "symbol is undefined" );

                return false;
            }

            result = symbol.ToDisplayString( UniqueNameFormat );

            if( GetSharpObjectType( symbol ) == SharpObjectType.Unknown )
            {
                if (!suppressMessages)
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

            if (!suppressMessages)
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

        private bool GetFQN( INamedTypeSymbol symbol, out string? result )
        {
            result = null;

            // non-generic types are simple...
            if( !symbol.IsGenericType )
            {
                result = symbol.ToDisplayString(UniqueNameFormat);
                return true;
            }

            var sb = new StringBuilder( symbol.ToDisplayString( GenericTypeFormat ) );

            sb.Append( "<" );

            // we identify type parameters by their ID from the database
            for( var argIdx = 0; argIdx < symbol.TypeArguments.Length; argIdx++ )
            {
                var argSymbol = symbol.TypeArguments[ argIdx ];

                if( argIdx > 0 )
                    sb.Append( ", " );

                if( Get<TypeDb>( argSymbol, out var argDb ) )
                    sb.Append( argDb!.SharpObjectID );
                else
                {
                    sb.Append( 0 );

                    _logger.Error<string, string>(
                        "Couldn't find type entity for type parameter '{0}' on symbol '{1}'",
                        argSymbol.Name,
                        symbol.ToDisplayString( UniqueNameFormat ) );

                    return false;
                }
            }

            sb.Append( ">" );

            result = sb.ToString();

            return true;
        }

        private bool GetFQN( ITypeParameterSymbol symbol, out string? result )
        {
            result = null;

            if( symbol.DeclaringType != null )
            {
                result = $"{symbol.DeclaringType.ToDisplayString( UniqueNameFormat )}::{symbol.Name}";
                return true;
            }

            if( symbol.DeclaringMethod != null )
            {
                result = $"{symbol.DeclaringMethod.ToDisplayString( UniqueNameFormat )}::{symbol.Name}";
                return true;
            }

            _logger.Error<string>(
                "ITypeParameterSymbol '{0}' is contained neither by an IMethodSymbol nor an INamedTypeSymbol",
                symbol.ToDisplayString( UniqueNameFormat ) );

            return false;
        }

        private bool GetFQN( IArrayTypeSymbol symbol, out string? result )
        {
            result = null;

            switch( symbol.ElementType )
            {
                case INamedTypeSymbol ntSymbol:
                    if( !GetFQN( ntSymbol, out var temp1 ) )
                        return false;

                    result = temp1;

                    return true;

                case ITypeParameterSymbol tpSymbol:
                    if( !GetFQN( tpSymbol, out var temp2 ) )
                        return false;

                    result = temp2;

                    return true;

                default:
                    _logger.Error<string>(
                        "ElementType of IArraySymbol '{0}' is neither an INamedTypeSymbol nor an ITypeParameterSymbol",
                        GetFullName(symbol));

                    return false;
            }
        }

        private bool GetFQN( IMethodSymbol symbol, out string? result )
        {
            result = null;

            // get the method name without the parentheses -- we'll add them
            // as we add the arguments
            var sb = new StringBuilder( symbol
                .ToDisplayString( UniqueNameFormat )
                .Replace( "()", string.Empty ) );

            sb.Append( symbol.Parameters.Length == 0 ? "(" : "( " );

            if( !AddParametersToFQN( symbol, sb, symbol.Parameters ) ) 
                return false;

            sb.Append(symbol.Parameters.Length == 0 ? ")" : " )");
            result = sb.ToString();

            return true;
        }

        private bool GetFQN( IPropertySymbol symbol, out string? result )
        {
            result = null;

            // get the property name without any brackets -- we'll add them
            // as we add the parameters
            var sb = new StringBuilder( symbol
                .ToDisplayString( UniqueNameFormat )
                .Replace( "[]", string.Empty ) );

            if( symbol.Parameters.Length > 0 )
                sb.Append( "[   " );

            if (!AddParametersToFQN(symbol, sb, symbol.Parameters))
                return false;

            if ( symbol.Parameters.Length > 0 )
                sb.Append( " ]" );

            result = sb.ToString();

            return true;
        }

        private bool AddParametersToFQN(
            ISymbol parentSymbol,
            StringBuilder sb,
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
                if( Get<TypeDb>( argSymbol, out var argDb ) )
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
            var symbolType = symbol.GetType();

            foreach( var factory in _factories )
            {
                if( factory.SymbolType.IsAssignableFrom( symbolType ) )
                    return factory.SharpObjectType;
            }

            return SharpObjectType.Unknown;
        }

        public List<SharpObjectType> GetSharpObjectType<TEntity>()
            where TEntity : ISharpObject
            => _factories.Where( f => typeof(TEntity).IsAssignableFrom( f.EntityType ) )
                .Select( f => f.SharpObjectType )
                .ToList();

        #endregion

        public RoslynDbContext DbContext { get; }

        #region Entity methods

        public bool InDatabase<TEntity>( ISymbol? symbol )
            where TEntity : class, ISharpObject
        {
            if( symbol == null )
                return false;

            foreach( var factory in _factories.Where( f => f.IsAssignableTo<TEntity>() ) )
            {
                if( factory.InDatabase( symbol ) )
                    return true;
            }

            return false;
        }

        public bool Get<TEntity>( ISymbol? symbol, out TEntity? result )
            where TEntity : class, ISharpObject
        {
            result = null;

            if( symbol == null )
                return false;

            // try to retrieve an instance of TEntity from every EntityFactory which claims
            // it can get entities assignable to TEntity
            foreach( var factory in _factories.Where( f => f.IsAssignableTo<TEntity>() ) )
            {
                if( !factory.InDatabase( symbol ) ) 
                    continue;
                
                factory.Get( symbol, out var innerResult );
                result = (TEntity) innerResult!;

                return true;
            }

            _logger.Error<string>( "Couldn't find an entity in the database for '{0}'", GetFullName( symbol ) );

            return false;
        }

        public bool Create<TEntity>( ISymbol? symbol, out TEntity? result )
            where TEntity : class, ISharpObject
        {
            result = null;

            if( symbol == null )
                return false;

            // if we're not creating an entity a factory which will retrieve an
            // entity that derives from the target TEntity is okay
            var factory = _factories.FirstOrDefault( f => f.CanCreate<TEntity>() );

            if( factory == null )
            {
                _logger.Error(
                    "Couldn't find a factory which retrieves {0} entities and can process the provided ISymbol {1}",
                    typeof(TEntity),
                    symbol.GetType() );

                return false;
            }

            if( !factory.Create( symbol, out var innerResult ) )
                return false;

            result = (TEntity) innerResult!;

            return true;
        }

        #endregion

        #region SharpObject methods

        internal bool SharpObjectInDatabase( ISymbol symbol )
        {
            if (!GetUniqueName(symbol, out var fqn, true))
                return false;

            if( GetSharpObjectType( symbol ) == SharpObjectType.Unknown )
                return false;

            return DbContext.SharpObjects.Any( x => x.FullyQualifiedName == fqn );
        }

        internal bool GetSharpObject( ISymbol symbol, out SharpObject? result )
        {
            result = null;

            if (!GetUniqueName(symbol, out var fqn))
            {
                _logger.Error<string>("Couldn't generate unique name of '{0}'",
                    symbol.ToDisplayString(UniqueNameFormat));

                return false;
            }

            if( GetSharpObjectType( symbol ) == SharpObjectType.Unknown )
            {
                _logger.Error<string>("Unknown SharpObjectType '{0}'", fqn);

                return false;
            }

            result = DbContext.SharpObjects.FirstOrDefault( x => x.FullyQualifiedName == fqn );

            if( result != null ) 
                return true;

            _logger.Error<string>( "Couldn't find SharpObject for '{0}' in the database", fqn );

            return false;
        }

        internal bool CreateSharpObject( ISymbol symbol, out SharpObject? result )
        {
            result = null;

            if (!GetUniqueName(symbol, out var fqn))
            {
                _logger.Error<string>("Couldn't generate unique name of '{0}'",
                    symbol.ToDisplayString(UniqueNameFormat));

                return false;
            }

            var soType = GetSharpObjectType( symbol );

            if (soType == SharpObjectType.Unknown)
            {
                _logger.Error<string>("Unknown SharpObjectType '{0}'", fqn);

                return false;
            }

            if ( DbContext.SharpObjects.Any( x => x.FullyQualifiedName == fqn ) )
            {
                _logger.Error<string>( "Duplicate SharpObject ({0})", GetFullName( symbol ) );

                return false;
            }

            result = new SharpObject
            {
                FullyQualifiedName = fqn!,
                Name = GetName( symbol ),
                SharpObjectType = soType,
                Synchronized = true
            };

            DbContext.SharpObjects.Add( result );

            return true;
        }

#endregion

        #region Methods for marking objects as synchronized or unsynchronized

        public void MarkSharpObjectUnsynchronized<TEntity>( bool saveChanges = false )
            where TEntity : class, ISharpObject
        {
            var entityType = typeof(TEntity);

            // update the underlying DocObject
            var docObjTypes = GetSharpObjectType<TEntity>();

            DbContext.SharpObjects.Where( x => docObjTypes.Any( z => z == x.SharpObjectType ) )
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