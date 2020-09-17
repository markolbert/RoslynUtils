using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn
{
    public class MethodFactory : EntityFactory<IMethodSymbol, MethodDb>
    {
        public MethodFactory( IJ4JLogger logger ) 
            : base( SharpObjectType.Method, logger )
        {
        }

        protected override bool GetEntitySymbol( ISymbol? symbol, out IMethodSymbol? result )
        {
            result = symbol as IMethodSymbol;

            return result != null;
        }

        protected override bool CreateNewEntity( IMethodSymbol symbol, out MethodDb? result )
        {
            result = new MethodDb();

            return true;
        }

        protected override bool ConfigureEntity( IMethodSymbol symbol, MethodDb newEntity )
        {
            if( !base.ConfigureEntity( symbol, newEntity ) )
                return false;

            newEntity!.Accessibility = symbol.DeclaredAccessibility;
            newEntity.DeclarationModifier = symbol.GetDeclarationModifier();
            newEntity.Kind = symbol.MethodKind;
            newEntity.ReturnsByRef = symbol.ReturnsByRef;
            newEntity.ReturnsByRefReadOnly = symbol.ReturnsByRefReadonly;
            newEntity.IsAbstract = symbol.IsAbstract;
            newEntity.IsExtern = symbol.IsExtern;
            newEntity.IsOverride = symbol.IsOverride;
            newEntity.IsReadOnly = symbol.IsReadOnly;
            newEntity.IsSealed = symbol.IsSealed;
            newEntity.IsStatic = symbol.IsStatic;
            newEntity.IsVirtual = symbol.IsVirtual;

            return true;
        }
    }
}
