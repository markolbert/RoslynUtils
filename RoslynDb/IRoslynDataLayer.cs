﻿using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn
{
    public interface IRoslynDataLayer
    {
        #region clearing synchronization

        void MarkUnsynchronized<TEntity>(bool saveChanges = true)
            where TEntity : class, ISynchronized;

        void MarkSharpObjectUnsynchronized<TEntity>( bool saveChanges = true )
            where TEntity : class, ISharpObject;

#endregion

        void SaveChanges();

        #region SharpObject

        bool SharpObjectInDatabase<TEntity>( ISymbol symbol )
            where TEntity : class, ISharpObject;

        ISharpObject? GetDescendantEntity<TEntity>( ISymbol symbol )
            where TEntity : class, ISharpObject;

        #endregion

        #region Assembly

        AssemblyDb? GetAssembly( IAssemblySymbol symbol, bool createIfMissing = false, bool updateExisting = false );
        bool UpdateAssembly( IAssemblySymbol symbol, AssemblyDb entity );

        InScopeAssemblyInfo? GetInScopeAssemblyInfo(
            CompiledProject project,
            bool createIfMissing = false,
            bool updateExisting = false );

        bool UpdateInScopeAssemblyInfo( CompiledProject project, InScopeAssemblyInfo entity );

        #endregion

        #region Namespace

        NamespaceDb? GetNamespace( INamespaceSymbol symbol, bool createIfMissing = false, bool updateExisting = false);
        bool UpdateNamespace( INamespaceSymbol symbol, NamespaceDb entity );

        #endregion

        AssemblyNamespaceDb? GetAssemblyNamespace(
            AssemblyDb assemblyDb,
            NamespaceDb nsDb,
            bool createIfMissing = false);

        #region Types

        #region ImplementableType

        FixedTypeDb? GetFixedType( INamedTypeSymbol symbol, bool createIfMissing = false, bool updateExisting = false);
        bool UpdateFixedType( INamedTypeSymbol symbol, FixedTypeDb entity );
        GenericTypeDb? GetGenericType( INamedTypeSymbol symbol, bool createIfMissing = false, bool updateExisting = false);
        bool UpdateGenericType( INamedTypeSymbol symbol, GenericTypeDb entity );
        ImplementableTypeDb? GetImplementableType( INamedTypeSymbol symbol, bool createIfMissing = false, bool updateExisting = false);
        bool UpdateImplementableType( INamedTypeSymbol symbol, ImplementableTypeDb entity );

        #endregion

        #region ParametricType

        ParametricTypeDb? GetParametricType( ITypeParameterSymbol symbol, bool createIfMissing = false, bool updateExisting = false);
        bool UpdateParametricType( ITypeParameterSymbol symbol, ParametricTypeDb entity );

        #endregion

        #region ParametricMethodType

        ParametricMethodTypeDb? GetParametricMethodType( ITypeParameterSymbol symbol, bool createIfMissing = false, bool updateExisting = false);
        bool UpdateParametricMethodType( ITypeParameterSymbol symbol, ParametricMethodTypeDb entity );

        #endregion

        BaseTypeDb? GetUnspecifiedParametricType( ITypeParameterSymbol symbol, bool createIfMissing = false, bool updateExisting = false);
        bool UpdateUnspecifiedParametricType( ITypeParameterSymbol symbol, BaseTypeDb entity );

        #region ArrayType

        BaseTypeDb? GetArrayType( IArrayTypeSymbol symbol, bool createIfMissing = false, bool updateExisting = false);
        bool UpdateArrayType( IArrayTypeSymbol symbol, ArrayTypeDb entity );

#endregion

        BaseTypeDb? GetUnspecifiedType( ITypeSymbol symbol, bool createIfMissing = false, bool updateExisting = false);
        bool UpdateUnspecifiedType( ITypeSymbol symbol, BaseTypeDb entity );

        #endregion

        #region Method

        MethodDb? GetMethod( IMethodSymbol symbol, bool createIfMissing = false, bool updateExisting = false);
        bool UpdateMethod( IMethodSymbol symbol, MethodDb entity );
        ArgumentDb? GetArgument( IParameterSymbol symbol, bool createIfMissing = false, bool updateExisting = false );
        bool UpdateArgument( IParameterSymbol symbol, ArgumentDb entity );

        #endregion

        #region Property

        PropertyDb? GetProperty( IPropertySymbol symbol, bool createIfMissing = false, bool updateExisting = false);
        bool UpdateProperty( IPropertySymbol symbol, PropertyDb entity );
        PropertyParameterDb? GetPropertyParameter( IParameterSymbol symbol, bool createIfMissing = false,
            bool updateExisting = false );
        bool UpdatePropertyParameter( IParameterSymbol symbol, PropertyParameterDb entity );

        #endregion

        #region Field

        FieldDb? GetField( IFieldSymbol symbol, bool createIfMissing = false, bool updateExisting = false );
        bool UpdateField( IFieldSymbol symbol, FieldDb entity );

        TypeAncestorDb? GetTypeAncestor(BaseTypeDb typeDb, ImplementableTypeDb ancestorDb,
            bool createIfMissing = false);

        TypeArgumentDb? GetTypeArgument(GenericTypeDb genericDb, ITypeSymbol symbol, int ordinal,
            bool createIfMissing = false);
#endregion
    }
}