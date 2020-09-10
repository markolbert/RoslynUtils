using System;
using System.Collections.Generic;
using System.Linq;

namespace J4JSoftware.Roslyn
{
    public class TopologicallySortableCollection<T>
        where T : class, IEquatable<T>
    {
        private readonly HashSet<T> _nodes = new HashSet<T>();
        private readonly HashSet<(T start, T end)> _edges = new HashSet<(T start, T end)>();

        public void Clear()
        {
            _nodes.Clear();
            _edges.Clear();
        }

        public bool Add( T start, T end )
        {
            var edge = ( start, end );

            if( start == end || _edges.Any( x => x == edge ) )
                return false;

            if( _nodes.Any( x => x == start ) )
                return false;

            _nodes.Add( start );
            _edges.Add( edge );

            return true;
        }

        public void Remove( T toRemove )
        {
            if( _nodes.Contains( toRemove ) )
                _nodes.Remove( toRemove );

            var edgesToRemove = new List<(T start, T end)>();

            foreach( var edge in _edges )
            {
                if( edge.start == toRemove || edge.end == toRemove )
                    edgesToRemove.Add( edge );
            }

            foreach( var edgeToRemove in edgesToRemove )
            {
                _edges.Remove( edgeToRemove );
            }
        }

        public bool Sort(out List<T>? result )
        {
            result = null;

            if( !_nodes.Any() || !_edges.Any() )
                return false;

            // Empty list that will contain the sorted elements
            var retVal = new Stack<T>();

            // Set of all nodes with no incoming edges
            var noIncomingEdges =
                new HashSet<T>(_nodes.Where(n => _edges.All(e => e.end.Equals(n) == false)));

            // while noIncomingEdges is non-empty do
            while (noIncomingEdges.Any())
            {
                //  remove a node from noIncomingEdges
                var nodeToRemove = noIncomingEdges.First();
                noIncomingEdges.Remove(nodeToRemove);

                // add removed node to stack
                retVal.Push(nodeToRemove);

                // for each targetNode with an edge from nodeToRemove to targetNode do
                foreach (var edge in _edges.Where(e => e.start.Equals(nodeToRemove)).ToList())
                {
                    var targetNode = edge.end;

                    // remove edge from the graph
                    _edges.Remove(edge);

                    // if targetNode has no other incoming edges then
                    if (_edges.All(me => me.end.Equals(targetNode) == false))
                    {
                        // insert targetNode into noIncomingEdges
                        noIncomingEdges.Add(targetNode);
                    }
                }
            }

            if (_edges.Any())
                return false;

            result = retVal.ToList();

            return true;
        }
    }
}