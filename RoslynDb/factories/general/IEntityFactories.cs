using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn
{
    public interface IEntityFactories : ISymbolFullName
    {
        RoslynDbContext DbContext { get; }

        bool GetUniqueName( ISymbol? symbol, out string result );
        string GetName( ISymbol symbol );

        SharpObjectType GetSharpObjectType(ISymbol symbol);

        SharpObjectType GetSharpObjectType<TEntity>()
            where TEntity : ISharpObject;

        bool CanProcess<TEntity>( ISymbol symbol, bool createIfMissing )
            where TEntity : class, ISharpObject;

        void MarkSharpObjectUnsynchronized<TEntity>( bool saveChanges = false )
            where TEntity : class, ISharpObject;

        void MarkUnsynchronized<TEntity>(bool saveChanges = false)
            where TEntity : class, ISynchronized;

        void MarkSynchronized<TEntity>( TEntity entity )
            where TEntity : class, ISharpObject;

        bool RetrieveSharpObject( ISymbol symbol, out SharpObject? result, bool createIfMissing = false );
        bool Retrieve<TEntity>( ISymbol symbol, out TEntity? result, bool createIfMissing = false )
            where TEntity : class, ISharpObject;
    }
}