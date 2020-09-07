using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn
{
    public abstract class ImplementableTypeEntityFactory<TSymbol, TEntity> : TypeEntityFactory<TSymbol, TEntity>
        where TSymbol : class, INamedTypeSymbol
        where TEntity : ImplementableTypeDb
    {
        protected ImplementableTypeEntityFactory( IJ4JLogger logger ) 
            : base( logger )
        {
        }

        protected override bool PostProcessEntitySymbol( TSymbol symbol, TEntity newEntity )
        {
            if( !base.PostProcessEntitySymbol( symbol, newEntity ) )
                return false;

            newEntity.DeclarationModifier = symbol.GetDeclarationModifier();

            return true;
        }
    }
}