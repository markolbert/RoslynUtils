using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Threading;
using J4JSoftware.Logging;
using J4JSoftware.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace J4JSoftware.Roslyn
{
    public class SingleWalker : TopoAction<CompiledProject>, ISingleWalker
    {
        private readonly ISyntaxNodeSink _nodeSink;
        private readonly WalkerContext _context;

        public SingleWalker(
            ISyntaxNodeSink nodeSink,
            WalkerContext context,
            IJ4JLogger logger
        )
            : base( logger )
        {
            _nodeSink = nodeSink;
            _context = context;
        }

        protected override bool Initialize( IEnumerable<CompiledProject> compResults )
        {
            Logger.Information( "Starting syntax walking..." );

            if( !base.Initialize( compResults ) )
                return false;

            _context.SetCompiledProjects(compResults);

            return true;
        }

        protected override bool ProcessLoop( IEnumerable<CompiledProject> compResults )
        {
            var nodeStack = new Stack<SyntaxNode>();

            foreach (var compResult in compResults.SelectMany(cr => cr))
            {
                if (!_nodeSink.InitializeSink(compResult.Model))
                    return false;

                nodeStack.Clear();
                nodeStack.Push( compResult.RootSyntaxNode );

                TraverseInternal(nodeStack);
            }

            return true;
        }

        protected void TraverseInternal( Stack<SyntaxNode> nodeStack )
        {
            _nodeSink.OutputSyntaxNode( nodeStack );

            if( !_nodeSink.DrillIntoNode( nodeStack.Peek() ) )
                return;

            foreach( var childNode in nodeStack.Peek().ChildNodes() )
            {
                nodeStack.Push( childNode );

                TraverseInternal( nodeStack );
            }

            nodeStack.Pop();
        }

        protected override bool Finalize( IEnumerable<CompiledProject> inputData )
        {
            Logger.Information("...finished syntax walking");

            if ( !base.Finalize( inputData ) )
                return false;

            return _nodeSink.FinalizeSink(this);
        }
    }
}
