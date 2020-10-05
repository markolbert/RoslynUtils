using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace J4JSoftware.Roslyn
{
    public class SingleWalker : EnumerableProcessorBase<CompiledProject>, ISingleWalker
    {
        private readonly ISyntaxNodeSink _nodeSink;
        private readonly List<SyntaxKind> _ignoredNodeKinds = new List<SyntaxKind>();

        public SingleWalker(
            ISyntaxNodeSink nodeSink,
            IJ4JLogger logger
        )
            : base( logger )
        {
            _nodeSink = nodeSink;

            _ignoredNodeKinds.Add(SyntaxKind.UsingDirective);
            _ignoredNodeKinds.Add(SyntaxKind.QualifiedName);
        }

        protected override bool PreLoopInitialization( IEnumerable<CompiledProject> compResults )
        {
            Logger.Information( "Starting syntax walking..." );

            if( !base.PreLoopInitialization( compResults ) )
                return false;

            return true;
        }

        protected override bool ProcessLoop( IEnumerable<CompiledProject> compResults )
        {
            foreach (var compResult in compResults.SelectMany(cr => cr))
            {
                if (!_nodeSink.InitializeSink(compResult.Model))
                    return false;

                TraverseInternal(compResult.RootSyntaxNode);
            }

            return true;
        }

        protected void TraverseInternal( SyntaxNode node )
        {
            // don't re-visit nodes
            switch( _nodeSink.OutputSyntaxNode( node ) )
            {
                case NodeSinkResult.AlreadyProcessed:
                    Logger.Verbose<string>("Already processed SyntaxNode", node.ToString());
                    return;

                case NodeSinkResult.UnsupportedSyntaxNode:
                    Logger.Verbose<SyntaxKind>("Unsupported SyntaxNode ({0})", node.Kind());
                    return;
            }

            if ( !GetChildNodesToVisit( node, out var children ) )
                return;

            foreach( var childNode in children! )
            {
                TraverseInternal( childNode );
            }
        }

        protected override bool PostLoopFinalization( IEnumerable<CompiledProject> inputData )
        {
            Logger.Information("...finished syntax walking");

            if ( !base.PostLoopFinalization( inputData ) )
                return false;

            return _nodeSink.FinalizeSink(this);
        }

        private bool GetChildNodesToVisit( SyntaxNode node, out List<SyntaxNode>? result )
        {
            result = null;

            // we're interested in traversing almost everything that's within scope
            // except for node types that we know don't lead any place interesting
            if (_ignoredNodeKinds.Any(nk => nk == node.Kind()))
                return false;

            result = node.ChildNodes()
                .Where(n => _ignoredNodeKinds.All(i => i != n.Kind()))
                .ToList();

            return true;
        }
    }
}
