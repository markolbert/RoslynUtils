using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn
{
    public interface IEntityInfo
    {
        ISymbol Symbol { get; }
        ISymbol EntitySymbol { get; }
        object EntityObject { get; }
        bool IsNew { get; }
        SharpObjectType Type { get; }
    }

    public interface IEntityInfo<out TEntity> : IEntityInfo
        where TEntity : class, ISharpObject
    {
        TEntity Entity { get; }
    }

    public class EntityInfo<TEntity> : IEntityInfo<TEntity>
        where TEntity : class, ISharpObject
    {
        public ISymbol Symbol { get; internal set; }
        public ISymbol EntitySymbol { get; internal set; }
        public TEntity Entity { get; internal set; }
        public bool IsNew { get; internal set; }
        public SharpObjectType Type { get; internal set; }

        object IEntityInfo.EntityObject => Entity;
    }
}