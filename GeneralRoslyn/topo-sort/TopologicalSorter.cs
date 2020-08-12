﻿using System;
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
        public static bool Sort<TNode>(
            IEnumerable<TNode> items,
            out List<TNode>? result
        )
            where TNode : class, ITopologicalSort<TNode>
        {
            result = null;

            var nodes = new HashSet<TNode>();
            var edges = new HashSet<(TNode start, TNode end)>();

            foreach( var item in items )
            {
                nodes.Add( item );

                var predecessor = nodes.FirstOrDefault( n => Equals( item, n.Predecessor ) );

                if( predecessor != null )
                    edges.Add( ( item, predecessor ) );
            }

            // Empty list that will contain the sorted elements
            var retVal = new Stack<TNode>();

            // Set of all nodes with no incoming edges
            var noIncomingEdges =
                new HashSet<TNode>(nodes.Where(n => edges.All(e => e.end.Equals(n) == false)));

            // while noIncomingEdges is non-empty do
            while (noIncomingEdges.Any())
            {
                //  remove a node from noIncomingEdges
                var nodeToRemove = noIncomingEdges.First();
                noIncomingEdges.Remove(nodeToRemove);

                // add removed node to stack
                retVal.Push(nodeToRemove);

                // for each targetNode with an edge from nodeToRemove to targetNode do
                foreach (var edge in edges.Where(e => e.start.Equals(nodeToRemove)).ToList())
                {
                    var targetNode = edge.end;

                    // remove edge from the graph
                    edges.Remove(edge);

                    // if targetNode has no other incoming edges then
                    if (edges.All(me => me.end.Equals(targetNode) == false))
                    {
                        // insert targetNode into noIncomingEdges
                        noIncomingEdges.Add(targetNode);
                    }
                }
            }

            if( edges.Any() )
                return false;

            result = retVal.Reverse().ToList();

            return true;
        }
    }
}
