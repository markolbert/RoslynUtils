#region license

// Copyright 2021 Mark A. Olbert
// 
// This library or program 'RoslynDb' is free software: you can redistribute it
// and/or modify it under the terms of the GNU General Public License as
// published by the Free Software Foundation, either version 3 of the License,
// or (at your option) any later version.
// 
// This library or program is distributed in the hope that it will be useful, but
// WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// General Public License for more details.
// 
// You should have received a copy of the GNU General Public License along with
// this library or program.  If not, see <https://www.gnu.org/licenses/>.

#endregion

using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn
{
    public interface IRoslynDataLayer
    {
        void SaveChanges();

        AssemblyNamespaceDb? GetAssemblyNamespace(
            AssemblyDb assemblyDb,
            NamespaceDb nsDb,
            bool createIfMissing = false );

        #region clearing synchronization

        void MarkUnsynchronized<TEntity>( bool saveChanges = true )
            where TEntity : class, ISynchronized;

        void MarkSharpObjectUnsynchronized<TEntity>( bool saveChanges = true )
            where TEntity : class, ISharpObject;

        #endregion

        #region SharpObject

        bool SharpObjectInDatabase<TEntity>( ISymbol symbol )
            where TEntity : class, ISharpObject;

        public void LoadSharpObject<TEntity>( TEntity entity )
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

        NamespaceDb? GetNamespace( INamespaceSymbol symbol, bool createIfMissing = false, bool updateExisting = false );
        bool UpdateNamespace( INamespaceSymbol symbol, NamespaceDb entity );

        #endregion

        #region Types

        #region ImplementableType

        FixedTypeDb? GetFixedType( INamedTypeSymbol symbol, bool createIfMissing = false, bool updateExisting = false );
        bool UpdateFixedType( INamedTypeSymbol symbol, FixedTypeDb entity );

        GenericTypeDb? GetGenericType( INamedTypeSymbol symbol, bool createIfMissing = false,
            bool updateExisting = false );

        bool UpdateGenericType( INamedTypeSymbol symbol, GenericTypeDb entity );

        ImplementableTypeDb? GetImplementableType( INamedTypeSymbol symbol, bool createIfMissing = false,
            bool updateExisting = false );

        bool UpdateImplementableType( INamedTypeSymbol symbol, ImplementableTypeDb entity );

        #endregion

        #region ParametricType

        ParametricTypeDb? GetParametricType( ITypeParameterSymbol symbol, bool createIfMissing = false,
            bool updateExisting = false );

        bool UpdateParametricType( ITypeParameterSymbol symbol, ParametricTypeDb entity );

        #endregion

        #region ParametricMethodType

        ParametricMethodTypeDb? GetParametricMethodType( ITypeParameterSymbol symbol, bool createIfMissing = false,
            bool updateExisting = false );

        bool UpdateParametricMethodType( ITypeParameterSymbol symbol, ParametricMethodTypeDb entity );

        #endregion

        BaseTypeDb? GetUnspecifiedParametricType( ITypeParameterSymbol symbol, bool createIfMissing = false,
            bool updateExisting = false );

        bool UpdateUnspecifiedParametricType( ITypeParameterSymbol symbol, BaseTypeDb entity );

        #region ArrayType

        BaseTypeDb? GetArrayType( IArrayTypeSymbol symbol, bool createIfMissing = false, bool updateExisting = false );
        bool UpdateArrayType( IArrayTypeSymbol symbol, ArrayTypeDb entity );

        #endregion

        BaseTypeDb? GetUnspecifiedType( ITypeSymbol symbol, bool createIfMissing = false, bool updateExisting = false );
        bool UpdateUnspecifiedType( ITypeSymbol symbol, BaseTypeDb entity );
        void LoadProperties( ImplementableTypeDb typeDb );

        #endregion

        #region Method

        MethodDb? GetMethod( IMethodSymbol symbol, bool createIfMissing = false, bool updateExisting = false );
        bool UpdateMethod( IMethodSymbol symbol, MethodDb entity );
        ArgumentDb? GetArgument( IParameterSymbol symbol, bool createIfMissing = false, bool updateExisting = false );
        bool UpdateArgument( IParameterSymbol symbol, ArgumentDb entity );
        void LoadMethodArguments( MethodDb methodDb );

        #endregion

        #region Property

        PropertyDb? GetProperty( IPropertySymbol symbol, bool createIfMissing = false, bool updateExisting = false );
        bool UpdateProperty( IPropertySymbol symbol, PropertyDb entity );

        PropertyParameterDb? GetPropertyParameter( IParameterSymbol symbol, bool createIfMissing = false,
            bool updateExisting = false );

        bool UpdatePropertyParameter( IParameterSymbol symbol, PropertyParameterDb entity );

        #endregion

        #region Field

        FieldDb? GetField( IFieldSymbol symbol, bool createIfMissing = false, bool updateExisting = false );
        bool UpdateField( IFieldSymbol symbol, FieldDb entity );

        TypeAncestorDb? GetTypeAncestor( BaseTypeDb typeDb, ImplementableTypeDb ancestorDb,
            bool createIfMissing = false );

        TypeArgumentDb? GetTypeArgument( GenericTypeDb genericDb, ITypeSymbol symbol, int ordinal,
            bool createIfMissing = false );

        #endregion

        #region Event

        EventDb? GetEvent( IEventSymbol symbol, bool createIfMissing = false, bool updateExisting = false );
        bool UpdateEvent( IEventSymbol symbol, EventDb entity );

        #endregion

        #region Attributes

        ISharpObject? GetAttributableEntity( ISymbol symbol );

        AttributeDb? GetAttribute<TEntity>(
            TEntity targetObjDb,
            AttributeData attrData,
            bool createIfMissing = false )
            where TEntity : class, ISharpObject;

        AttributeArgumentDb? GetAttributeArgument(
            AttributeDb attrDb,
            AttributeData attrData,
            string propName,
            bool createIfMissing = false );

        AttributeArgumentDb? GetAttributeArgument(
            AttributeDb attrDb,
            AttributeData attrData,
            int ctorArgOrdinal,
            bool createIfMissing = false );

        #endregion
    }
}