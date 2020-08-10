using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace J4JSoftware.Roslyn
{
    public interface ITopologicalSort
    {
        void GetPredecessor( out object? result );
        void SetPredecessor( object? value );
    }

    public interface ITopologicalSort<TNode> : ITopologicalSort, IEquatable<TNode>
        where TNode : class
    {
        void GetPredecessor( out TNode? result );
        void SetPredecessor( TNode? value );
    }
}
