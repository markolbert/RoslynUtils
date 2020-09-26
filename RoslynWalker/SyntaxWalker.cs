using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn
{
    public abstract class SyntaxWalker<TSymbol> : EnumerableProcessorBase<CompiledProject>, ISyntaxWalker<TSymbol>
        where TSymbol : class, ISymbol
    {
        private readonly ISymbolSink _symbolSink;
        private readonly List<SyntaxNode> _visitedNodes = new List<SyntaxNode>();

        protected SyntaxWalker(
            ISymbolFullName symbolInfo,
            IDefaultSymbolSink defaultSymbolSink,
            ExecutionContext context,
            IJ4JLogger logger,
            ISymbolSink<TSymbol>? symbolSink = null
        )
            : base( logger )
        {
            SymbolInfo = symbolInfo;
            SymbolType = typeof(TSymbol);

            Context = context;

            if( symbolSink == null )
            {
                Logger.Error( "No ISymbolSink defined for symbol type '{0}'", typeof(TSymbol) );
                _symbolSink = defaultSymbolSink;
            }
            else _symbolSink = symbolSink;
        }

        protected ISymbolFullName SymbolInfo { get; }
        protected ExecutionContext Context { get; }

        public Type SymbolType { get; }

        protected override bool PreLoopInitialization( IEnumerable<CompiledProject> compResults )
        {
            if( !base.PreLoopInitialization( compResults ) )
                return false;

            if (!_symbolSink.InitializeSink(this))
                return false;

            Context.SetCompiledProjects( compResults );

            _visitedNodes.Clear();

            return true;
        }

        protected override bool ProcessLoop( IEnumerable<CompiledProject> compResults )
        {
            foreach (var compResult in compResults.SelectMany(cr => cr))
            {
                TraverseInternal(compResult.RootSyntaxNode, compResult);
            }

            return true;
        }

        protected void TraverseInternal( SyntaxNode node, CompiledFile context )
        {
            // don't re-visit nodes
            if( _visitedNodes.Any( vn => vn.Equals( node ) ) )
                return;

            _visitedNodes.Add( node );

            // we make no attempt to keep track of whether a symbol has already been processed.
            // that's the responsibility of the sink
            if( NodeReferencesSymbol( node, context, out var symbol ) )
                _symbolSink?.OutputSymbol( this, symbol! );

            if( !GetChildNodesToVisit( node, out var children ) )
                return;

            foreach( var childNode in children! )
            {
                TraverseInternal( childNode, context );
            }
        }

        protected override bool PostLoopFinalization( IEnumerable<CompiledProject> inputData )
        {
            if( !base.PostLoopFinalization( inputData ) )
                return false;

            return _symbolSink.FinalizeSink(this);
        }

        protected abstract bool NodeReferencesSymbol( SyntaxNode node, CompiledFile context, out TSymbol? result );
        protected abstract bool GetChildNodesToVisit( SyntaxNode node, out List<SyntaxNode>? result );

        public bool Equals( ISyntaxWalker? other )
        {
            if( other == null )
                return false;

            return other.SymbolType == SymbolType;
        }
    }
}
