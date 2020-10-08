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
        private readonly ExecutionContext _context;

        public SingleWalker(
            ISyntaxNodeSink nodeSink,
            ExecutionContext context,
            IJ4JLogger logger
        )
            : base( logger )
        {
            _nodeSink = nodeSink;
            _context = context;
        }

        protected override bool PreLoopInitialization( IEnumerable<CompiledProject> compResults )
        {
            Logger.Information( "Starting syntax walking..." );

            if( !base.PreLoopInitialization( compResults ) )
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

        protected override bool PostLoopFinalization( IEnumerable<CompiledProject> inputData )
        {
            Logger.Information("...finished syntax walking");

            if ( !base.PostLoopFinalization( inputData ) )
                return false;

            return _nodeSink.FinalizeSink(this);
        }
    }
}
