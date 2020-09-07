using System;

namespace J4JSoftware.Roslyn
{
    public interface ITopologicalSort<TNode> : IEquatable<TNode>
        where TNode : class
    {
        TNode? Predecessor { get; set; }
    }
}
