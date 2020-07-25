using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using J4JSoftware.Logging;
using Microsoft.Build.Logging.StructuredLogger;

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

            return edges.Any() ? null : retVal;
        }

        public static string? CreateSequence<TNode>(
            IEnumerable<TNode> items,
            Func<TNode, IEnumerable<TNode>> predecessorExtractor,
            out List<TNode>? result
        )
            where TNode : IEquatable<TNode>
        {
            result = null;

            var nodes = new HashSet<TNode>();
            var edges = new HashSet<(TNode start, TNode end)>();

            foreach( var item in items )
            {
                nodes.Add( item );

                foreach( var predecessor in predecessorExtractor( item ) )
                {
                    edges.Add( ( item, predecessor ) );
                }
            }

            result = TopologicalSorter.Sort(nodes, edges).ToList();

            return result.Count == 0
                ? $"Couldn't determine sequence. Check your {typeof(RoslynProcessorAttribute)} attributes"
                : null;
        }

        public static string? CreateSequence<TNode>( IEnumerable<TNode> items, out List<TNode>? result )
            where TNode : IEquatable<TNode>
        {
            result = null;

            var nodeType = typeof(TNode);

            // validate that all the items either are not decorated with RoslynProcessorAttributes
            // or, if they are, the PredecessorType properties are all consistent
            var itemList = items.Where( x => x != null ).ToList();

            if( itemList.Count == 0 )
                return "No items provided from which to create a sequence";

            if( !itemList
                .SelectMany( x => x!.GetType().GetCustomAttributes<RoslynProcessorAttribute>() )
                .All( x => nodeType.IsAssignableFrom( x.PredecessorType ) ) )
                return
                    $"One or more of the items has a {typeof(RoslynProcessorAttribute)} which refers to a Type not assignable to {typeof(TNode)}";

            var mesg = CreateSequence( itemList, x =>
                {
                    var retVal = new List<TNode>();

                    foreach( var depAttr in x.GetType()
                        .GetCustomAttributes<RoslynProcessorAttribute>() )
                    {
                        var predecessor = itemList.FirstOrDefault( y => y!.GetType() == depAttr.PredecessorType );

                        if( predecessor == null )
                            throw new ArgumentOutOfRangeException(
                                $"Could not find Type {depAttr.PredecessorType.Name} updater in provided collection" );

                        retVal.Add( predecessor );
                    }

                    return retVal;
                },
                out var innerResult );

            if( mesg == null )
                result = innerResult;

            return mesg;
        }
    }
}
