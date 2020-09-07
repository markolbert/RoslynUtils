using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn.Deprecated
{
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