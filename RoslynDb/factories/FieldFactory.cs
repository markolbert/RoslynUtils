using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn
{
    public class FieldFactory : EntityFactory<IFieldSymbol, FieldDb>
    {
        public FieldFactory( IJ4JLogger logger ) 
            : base( logger )
        {
        }

        protected override bool GetEntitySymbol( ISymbol? symbol, out IFieldSymbol? result )
        {
            result = symbol as IFieldSymbol;

            return result != null;
        }

        protected override bool CreateNewEntity( IFieldSymbol symbol, out FieldDb? result )
        {
            result = new FieldDb();

            return true;
        }

        protected override bool ConfigureEntity( IFieldSymbol symbol, FieldDb newEntity )
        {
            if( !base.ConfigureEntity( symbol, newEntity ) )
                return false;

            newEntity!.Accessibility = symbol.DeclaredAccessibility;
            newEntity.DeclarationModifier = symbol.GetDeclarationModifier();
            newEntity.IsAbstract = symbol.IsAbstract;
            newEntity.IsExtern = symbol.IsExtern;
            newEntity.IsOverride = symbol.IsOverride;
            newEntity.IsReadOnly = symbol.IsReadOnly;
            newEntity.IsSealed = symbol.IsSealed;
            newEntity.IsStatic = symbol.IsStatic;
            newEntity.IsVirtual = symbol.IsVirtual;
            newEntity.IsVolatile = symbol.IsVolatile;

            return true;
        }
    }
}
