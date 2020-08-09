using System;
using System.Collections.Generic;

namespace J4JSoftware.Roslyn
{
    public interface ITopologicallySorted<TNode>
        where TNode : IEquatable<TNode>
    {
        public List<TNode> ExecutionSequence { get; }
    }
}