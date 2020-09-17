using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn
{
    public abstract class ImplementableTypeEntityFactory<TSymbol, TEntity> : TypeEntityFactory<TSymbol, TEntity>
        where TSymbol : class, INamedTypeSymbol
        where TEntity : ImplementableTypeDb
    {
        protected ImplementableTypeEntityFactory( 
            SharpObjectType sharpObjType,
            IJ4JLogger logger 
            ) 
            : base( sharpObjType, logger )
        {
        }

        protected override bool ConfigureEntity( TSymbol symbol, TEntity newEntity )
        {
            if( !base.ConfigureEntity( symbol, newEntity ) )
                return false;

            newEntity.DeclarationModifier = symbol.GetDeclarationModifier();

            return true;
        }
    }
}