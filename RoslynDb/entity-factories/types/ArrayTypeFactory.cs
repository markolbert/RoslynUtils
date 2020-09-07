using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn.entityfactories
{
    public class ArrayTypeFactory : EntityFactory<IArrayTypeSymbol, TypeDb>
    {
        public ArrayTypeFactory( IJ4JLogger logger ) 
            : base( logger )
        {
        }

        protected override bool GetEntitySymbol( ISymbol symbol, out IArrayTypeSymbol? result )
        {
            result = symbol as IArrayTypeSymbol;

            return result != null;
        }

        protected override bool ValidateEntitySymbol(IArrayTypeSymbol symbol)
        {
            if (!base.ValidateEntitySymbol(symbol))
                return false;

            if (!Factories!.Retrieve<AssemblyDb>(symbol.ContainingAssembly, out _))
            {
                Logger.Error<string>("Couldn't find AssemblyDb entity in database for '{0}'",
                    Factories!.GetFullyQualifiedName(symbol));

                return false;
            }

            if (!Factories!.Retrieve<NamespaceDb>(symbol.ContainingNamespace, out _))
            {
                Logger.Error<string>("Couldn't find NamespaceDb entity in database for '{0}'",
                    Factories!.GetFullyQualifiedName(symbol));

                return false;
            }

            return true;
        }

        protected override bool CreateNewEntity( IArrayTypeSymbol symbol, out TypeDb? result )
        {
            result = null;

            switch( symbol.ElementType )
            {
                case INamedTypeSymbol ntSymbol:
                    result = ntSymbol.IsGenericType ? (ImplementableTypeDb) new GenericTypeDb() : new FixedTypeDb();
                    break;

                case ITypeParameterSymbol tpSymbol:
                    if( tpSymbol.DeclaringType != null )
                        result = new TypeParametricTypeDb();
                    else
                    {
                        if( tpSymbol.DeclaringMethod != null )
                            result = new MethodParametricTypeDb();
                        else
                            Logger.Error<string>(
                                "ITypeParameterSymbol is contained by neither an IMethodSymbol nor an INamedTypeSymbol",
                                Factories!.GetFullyQualifiedName( symbol.ElementType ) );
                    }

                    break;
            }

            return result != null;
        }

        protected override bool PostProcessEntitySymbol( IArrayTypeSymbol symbol, TypeDb newEntity )
        {
            if (!base.PostProcessEntitySymbol(symbol, newEntity))
                return false;

            if (!Factories!.Retrieve<AssemblyDb>(symbol.ElementType.ContainingAssembly, out var assemblyDb))
                return false;

            if (!Factories!.Retrieve<NamespaceDb>(symbol.ElementType.ContainingNamespace, out var nsDb))
                return false;

            if (assemblyDb!.SharpObjectID == 0)
                newEntity.Assembly = assemblyDb;
            else newEntity.AssemblyID = assemblyDb.SharpObjectID;

            if (nsDb!.SharpObjectID == 0)
                newEntity.Namespace = nsDb;
            else newEntity.NamespaceID = nsDb.SharpObjectID;

            newEntity.InDocumentationScope = assemblyDb.InScopeInfo != null;
            newEntity.Accessibility = symbol.DeclaredAccessibility;
            newEntity.Nature = symbol.TypeKind;

            if( newEntity is ImplementableTypeDb implTypeDb )
                implTypeDb.DeclarationModifier = symbol.ElementType.GetDeclarationModifier();

            return true;
        }
    }
}
