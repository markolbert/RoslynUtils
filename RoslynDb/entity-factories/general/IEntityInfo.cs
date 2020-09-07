using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn.Deprecated
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
}