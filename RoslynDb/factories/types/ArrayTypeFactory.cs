using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn.entityfactories
{
    public class ArrayTypeFactory : EntityFactory<IArrayTypeSymbol, BaseTypeDb>
    {
        public ArrayTypeFactory( IJ4JLogger logger ) 
            : base( SharpObjectType.ArrayType, logger )
        {
        }

        protected override bool GetEntitySymbol( ISymbol? symbol, out IArrayTypeSymbol? result )
        {
            result = symbol as IArrayTypeSymbol;

            return result != null;
        }

        protected override bool ValidateEntitySymbol( IArrayTypeSymbol symbol )
        {
            if( !base.ValidateEntitySymbol( symbol ) )
                return false;

            if( !Factories!.InDatabase<AssemblyDb>( symbol.ElementType.ContainingAssembly ) )
            {
                Logger.Error<string>( "Couldn't find AssemblyDb entity in database for '{0}'",
                    symbol.ToFullName() );

                return false;
            }

            if( !Factories!.InDatabase<NamespaceDb>( symbol.ElementType.ContainingNamespace ) )
            {
                Logger.Error<string>( "Couldn't find NamespaceDb entity in database for '{0}'",
                    symbol.ToFullName() );

                return false;
            }

            return true;
        }

        protected override bool CreateNewEntity( IArrayTypeSymbol symbol, out BaseTypeDb? result )
        {
            result = null;

            switch( symbol.ElementType )
            {
                case INamedTypeSymbol ntSymbol:
                    result = ntSymbol.IsGenericType ? (ImplementableTypeDb) new GenericTypeDb() : new FixedTypeDb();
                    break;

                case ITypeParameterSymbol tpSymbol:
                    if( tpSymbol.DeclaringType != null )
                        result = new ParametricTypeDb();
                    else
                    {
                        if( tpSymbol.DeclaringMethod != null )
                            result = new ParametricMethodTypeDb();
                        else
                            Logger.Error<string>(
                                "ITypeParameterSymbol is contained by neither an IMethodSymbol nor an INamedTypeSymbol",
                                symbol.ElementType.ToFullName() );
                    }

                    break;
            }

            return result != null;
        }

        protected override bool ConfigureEntity( IArrayTypeSymbol symbol, BaseTypeDb newEntity )
        {
            if (!base.ConfigureEntity(symbol, newEntity))
                return false;

            if (!Factories!.Get<AssemblyDb>(symbol.ElementType.ContainingAssembly, out var assemblyDb))
                return false;

            if (!Factories!.Get<NamespaceDb>(symbol.ElementType.ContainingNamespace, out var nsDb))
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
