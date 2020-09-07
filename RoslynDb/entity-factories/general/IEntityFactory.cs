using System;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn
{
    public interface IEntityFactory
    {
        Type EntityType { get; }
        IEntityFactories Factories { get; set; }
        bool CanProcess( ISymbol symbol );
        bool Retrieve( ISymbol symbol, out IEntityInfo? result, bool createIfMissing = false );
    }

    public interface IEntityFactory<TEntity> : IEntityFactory
        where TEntity : class, ISharpObject
    {
        bool Retrieve( ISymbol symbol, out EntityInfo<TEntity>? result, bool createIfMissing = false );
    }
}