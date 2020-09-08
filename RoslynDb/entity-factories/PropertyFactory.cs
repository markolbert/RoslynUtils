using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn
{
    public class PropertyFactory : EntityFactory<IPropertySymbol, PropertyDb>
    {
        public PropertyFactory( IJ4JLogger logger ) 
            : base( logger )
        {
        }

        protected override bool GetEntitySymbol( ISymbol? symbol, out IPropertySymbol? result )
        {
            result = symbol as IPropertySymbol;

            return result != null;
        }

        protected override bool CreateNewEntity( IPropertySymbol symbol, out PropertyDb? result )
        {
            result = new PropertyDb();

            return true;
        }

        protected override bool ConfigureEntity( IPropertySymbol symbol, PropertyDb newEntity )
        {
            if( !base.ConfigureEntity( symbol, newEntity ) )
                return false;

            newEntity!.GetAccessibility = symbol.GetMethod?.DeclaredAccessibility ?? Accessibility.NotApplicable;
            newEntity.SetAccessibility = symbol.SetMethod?.DeclaredAccessibility ?? Accessibility.NotApplicable;
            newEntity.DeclarationModifier = symbol.GetDeclarationModifier();
            newEntity.ReturnsByRef = symbol.ReturnsByRef;
            newEntity.ReturnsByRefReadOnly = symbol.ReturnsByRefReadonly;
            newEntity.IsAbstract = symbol.IsAbstract;
            newEntity.IsExtern = symbol.IsExtern;
            newEntity.IsIndexer = symbol.IsIndexer;
            newEntity.IsOverride = symbol.IsOverride;
            newEntity.IsReadOnly = symbol.IsReadOnly;
            newEntity.IsSealed = symbol.IsSealed;
            newEntity.IsStatic = symbol.IsStatic;
            newEntity.IsVirtual = symbol.IsVirtual;
            newEntity.IsWriteOnly = symbol.IsWriteOnly;

            return true;
        }
    }
}
