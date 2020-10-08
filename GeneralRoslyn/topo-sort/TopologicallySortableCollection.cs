﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;
using Serilog;

namespace J4JSoftware.Roslyn
{
    public enum TopologicalAdditionResult
    {
        AddedEntry,
        DuplicateEntry
    }

    public class TopologicallySortableCollection<T>
        where T : class, IEquatable<T>
    {
        private readonly IEqualityComparer<T>? _comparer;
        private readonly HashSet<T> _nodes = new HashSet<T>();
        private readonly HashSet<(T predecessorNode, T node)> _edges = new HashSet<(T predecessorNode, T node)>();

        public TopologicallySortableCollection( IEqualityComparer<T>? comparer = null )
        {
            _comparer = comparer;
        }

        public void Clear()
        {
            _nodes.Clear();
            _edges.Clear();
        }

        public List<T> Nodes => _nodes.ToList();

        public List<T> Edges( T? node = null )
        {
            if( node == null )
                return _nodes.Where( x => _edges.All( y => !NodesAreEqual(y.node, x) ) )
                    .Distinct()
                    .ToList();
            else
                return _edges.Where( x => NodesAreEqual(x.predecessorNode ,node) )
                    .Select( x => x.node )
                    .Distinct()
                    .ToList();
        }

        public bool HasNode( T node ) => _nodes.Any( n => NodesAreEqual(n ,node) );

        public bool HasEdge( T predecessorNode, T node )
        {
            var edge = ( predecessorNode, node );

            return _edges.Any( x => EdgesAreEqual(x ,edge) );
        }

        public virtual T Add( T unboundNode )
        {
            if( _nodes.All( x => !NodesAreEqual(x, unboundNode)) )
                _nodes.Add( unboundNode );

            return unboundNode;
        }

        public virtual T Add( T predecessorNode, T node )
        {
            if( node is IAssemblySymbol )
                System.Diagnostics.Debugger.Break();

            Add( predecessorNode );
            Add( node );

            var edge = ( predecessorNode, node );

            if( predecessorNode == node || _edges.Any( x => EdgesAreEqual( x, edge ) ) )
                return predecessorNode;

            _edges.Add( edge );

            return predecessorNode;
        }

        public void Remove( T toRemove )
        {
            if( _nodes.Contains( toRemove ) )
                _nodes.Remove( toRemove );

            var edgesToRemove = new List<(T start, T end)>();

            foreach( var edge in _edges )
            {
                if( NodesAreEqual(edge.predecessorNode, toRemove) || NodesAreEqual(edge.node, toRemove))
                    edgesToRemove.Add( edge );
            }

            foreach( var edgeToRemove in edgesToRemove )
            {
                _edges.Remove( edgeToRemove );
            }
        }

        public bool Sort(out List<T>? sorted, out List<(T node, T predecessor)>? remainingEdges  )
        {
            sorted = null;
            remainingEdges = null;

            switch( _nodes.Count )
            {
                case 0:
                    return false;

                case 1:
                    if( _edges.Count > 0 )
                        return false;

                    break;
            }

            // Empty list that will contain the sorted elements
            var retVal = new Stack<T>();

            // work with a copy of edges so we can keep re-sorting
            var edges = new HashSet<(T startOfEdge, T endOfEdge)>( _edges.ToArray() );

            // Set of all nodes with no incoming edges
            var noIncomingEdges =
                new HashSet<T>(_nodes.Where(n => edges.All(e => !NodesAreEqual(e.endOfEdge, n))));

            // while noIncomingEdges is non-empty do
            while (noIncomingEdges.Any())
            {
                //  remove a node from noIncomingEdges
                var nodeToRemove = noIncomingEdges.First();
                noIncomingEdges.Remove(nodeToRemove);

                // add removed node to stack
                retVal.Push(nodeToRemove);

                // for each targetNode with an edge from nodeToRemove to targetNode do
                foreach (var edge in edges.Where(e => NodesAreEqual(e.startOfEdge, nodeToRemove) ).ToList())
                {
                    var targetNode = edge.endOfEdge;

                    // remove edge from the graph
                    edges.Remove(edge);

                    // if targetNode has no other incoming edges then
                    if (edges.All(me => !NodesAreEqual(me.endOfEdge, targetNode) ))
                    {
                        // insert targetNode into noIncomingEdges
                        noIncomingEdges.Add(targetNode);
                    }
                }
            }

            remainingEdges = edges.ToList();

            if ( edges.Any() )
                return false;

            sorted = retVal.ToList();

            return true;
        }

        protected bool NodesAreEqual(T? x, T? y)
        {
            if (_comparer == null)
                return x == y;

            return _comparer.Equals(x, y);
        }

        protected bool EdgesAreEqual((T start, T end) x, (T start, T end) y)
        {
            if (_comparer == null)
                return x == y;

            return _comparer.Equals(x.start, y.start) && _comparer.Equals(x.end, y.end);
        }
    }
}