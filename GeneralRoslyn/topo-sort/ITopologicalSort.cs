using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace J4JSoftware.Roslyn
{
    public interface ITopologicalSort<TNode> : IEquatable<TNode>
        where TNode : class
    {
        TNode? Predecessor { get; set; }
    }
}
