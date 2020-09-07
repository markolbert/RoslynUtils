using System;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn
{
    public class SharpObjectTypeMapper : ISharpObjectTypeMapper
    {
        public SharpObjectType this [ ISymbol symbol ] 
        {
            get
            {
                return symbol switch
                {
                    IAssemblySymbol aSymbol => SharpObjectType.Assembly,
                    INamespaceSymbol nsSymbol => SharpObjectType.Namespace,
                    INamedTypeSymbol ntSymbol => ntSymbol.IsGenericType ? SharpObjectType.GenericType : SharpObjectType.FixedType,
                    IMethodSymbol mSymbol => SharpObjectType.Method,
                    ITypeParameterSymbol tpSymbol => SharpObjectType.ParametricType,
                    IPropertySymbol pSymbol => SharpObjectType.Property,
                    _ => SharpObjectType.Unknown
                };
            }
        }

        public SharpObjectType this[Type entityType]
        {
            get
            {
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
        }
    }
}