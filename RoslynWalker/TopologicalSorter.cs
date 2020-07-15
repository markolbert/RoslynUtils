using System;
using System.Collections.Generic;
using System.Linq;

namespace J4JSoftware.Roslyn
{
    // thanx to https://gist.github.com/Sup3rc4l1fr4g1l1571c3xp14l1d0c10u5/3341dba6a53d7171fe3397d13d00ee3f for the Kahn's
    // topological sorting algorithm!!
    public static class TopologicalSorter
    {
        public static Stack<TNode>? Sort<TNode>( 
            HashSet<TNode> nodes, 
            HashSet<(TNode start, TNode end)> edges
            ) 
            where TNode : IEquatable<TNode>
        {
            // Empty list that will contain the sorted elements
            var retVal = new Stack<TNode>();

            // Set of all nodes with no incoming edges
            var noIncomingEdges =
                new HashSet<TNode>( nodes.Where( n => edges.All( e => e.end.Equals( n ) == false ) ) );

            // while noIncomingEdges is non-empty do
            while( noIncomingEdges.Any() )
            {
                //  remove a node from noIncomingEdges
                var nodeToRemove = noIncomingEdges.First();
                noIncomingEdges.Remove( nodeToRemove );

                // add removed node to stack
                retVal.Push(nodeToRemove);

                // for each targetNode with an edge from nodeToRemove to targetNode do
                foreach( var edge in edges.Where( e => e.start.Equals( nodeToRemove ) ).ToList() )
                {
                    var targetNode = edge.end;

                    // remove edge from the graph
                    edges.Remove( edge );

                    // if targetNode has no other incoming edges then
                    if( edges.All( me => me.end.Equals( targetNode ) == false ) )
                    {
                        // insert targetNode into noIncomingEdges
                        noIncomingEdges.Add( targetNode );
                    }
                }
            }

            // if graph has remaining edges then
            if( edges.Any() )
            {
                // return error (graph has at least one cycle)
                return null;
            }
            else
            {
                // return a topologically sorted order
                return retVal;
            }
        }
    }
}
