using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn
{
    public interface IEntityFactories
    {
        ISharpObjectTypeMapper SharpObjectTypeMapper { get; }
        RoslynDbContext DbContext { get; }

        string GetFullyQualifiedName( ISymbol symbol );
        string GetName( ISymbol symbol );

        bool CanProcess<TEntity>( ISymbol symbol, bool createIfMissing )
            where TEntity : class, ISharpObject;

        void MarkUnsynchronized<TEntity>( bool saveChanges = false )
            where TEntity : class;

        void MarkSynchronized<TEntity>( TEntity entity )
            where TEntity : class, ISharpObject;

        bool RetrieveSharpObject( ISymbol symbol, out SharpObject? result, bool createIfMissing = false );
        bool Retrieve<TEntity>( ISymbol symbol, out TEntity? result, bool createIfMissing = false )
            where TEntity : class, ISharpObject;
    }
}