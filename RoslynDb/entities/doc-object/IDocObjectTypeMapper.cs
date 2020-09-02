using System;

namespace J4JSoftware.Roslyn
{
    public interface IDocObjectTypeMapper
    {
        DocObjectType GetDocObjectType<TEntity>();
        DocObjectType this[ Type entityType ] { get; }
    }
}