using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn.Deprecated
{
    public interface IEntityFactories : ISymbolFullName
    {
        RoslynDbContext DbContext { get; }

        bool GetUniqueName( ISymbol? symbol, out string result );
        string GetName( ISymbol symbol );

        SharpObjectType GetSharpObjectType(ISymbol symbol);

        SharpObjectType GetSharpObjectType<TEntity>()
            where TEntity : ISharpObject;

        void MarkSharpObjectUnsynchronized<TEntity>( bool saveChanges = false )
            where TEntity : class, ISharpObject;

        void MarkUnsynchronized<TEntity>(bool saveChanges = false)
            where TEntity : class, ISynchronized;

        void MarkSynchronized<TEntity>( TEntity entity )
            where TEntity : class, ISharpObject;

        bool GetSharpObject( ISymbol symbol, out SharpObject? result );
        bool CreateSharpObject( ISymbol symbol, out SharpObject? result );

        bool Get<TEntity>( ISymbol symbol, out TEntity? result )
            where TEntity : class, ISharpObject;
        bool Create<TEntity>(ISymbol symbol, out TEntity? result)
            where TEntity : class, ISharpObject;
    }
}