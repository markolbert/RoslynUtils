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
    public class SingleWalker : ISingleWalker
    {
        private readonly ISyntaxNodeSink _nodeSink;
        private readonly WalkerContext _context;
        private readonly IJ4JLogger? _logger;

        public SingleWalker(
            ISyntaxNodeSink nodeSink,
            WalkerContext context,
            IJ4JLogger? logger
        )
        {
            _nodeSink = nodeSink;
            _context = context;

            _logger = logger;
            _logger?.SetLoggedType( GetType() );
        }

        protected virtual bool Initialize( List<CompiledProject> projects )
        {
            _logger?.Information( "Starting syntax walking..." );

            _context.SetCompiledProjects(projects);

            return true;
        }

        public virtual bool Process( List<CompiledProject> compResults )
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

        protected virtual bool Finalize( List<CompiledProject> inputData )
        {
            _logger?.Information("...finished syntax walking");

            return _nodeSink.FinalizeSink(this);
        }

        public bool Equals( IAction<List<CompiledProject>>? other )
            => other switch
            {
                null => false,
                SingleWalker castOther => castOther.GetType() == GetType(),
                _ => false
            };

        bool IAction.Process( object src )
        {
            if( src is List<CompiledProject> castSrc )
                return Process( castSrc );

            _logger?.Error( "Expected a {0} but received a {1}", typeof(List<CompiledProject>), src.GetType() );

            return false;
        }
    }
}
