using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Threading;
using J4JSoftware.Logging;
using J4JSoftware.Utilities;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn
{
    public abstract class SyntaxWalker<TSymbol> : ISyntaxWalker
        where TSymbol : class, ISymbol
    {
        private readonly ISymbolSink _symbolSink;
        private readonly List<SyntaxNode> _visitedNodes = new List<SyntaxNode>();

        protected SyntaxWalker(
            string walkerName,
            ISymbolFullName symbolInfo,
            IDefaultSymbolSink defaultSymbolSink,
            WalkerContext context,
            IJ4JLogger? logger,
            ISymbolSink<TSymbol>? symbolSink = null
        )
        {
            Name = walkerName;
            Logger = logger;
            Logger?.SetLoggedType( GetType() );

            SymbolInfo = symbolInfo;
            SymbolType = typeof(TSymbol);

            Context = context;

            if( symbolSink == null )
            {
                Logger?.Error( "No ISymbolSink defined for symbol type '{0}'", typeof(TSymbol) );
                _symbolSink = defaultSymbolSink;
            }
            else _symbolSink = symbolSink;
        }

        protected IJ4JLogger? Logger { get; }
        protected ISymbolFullName SymbolInfo { get; }
        protected WalkerContext Context { get; }

        public Type SymbolType { get; }
        public string Name { get; }

        protected virtual bool Initialize( List<CompiledProject> projects )
        {
            Logger?.Information<string>( "Starting {0}...", Name );

            if (!_symbolSink.InitializeSink(this))
                return false;

            Context.SetCompiledProjects( projects );

            _visitedNodes.Clear();

            return true;
        }

        public virtual bool Process( List<CompiledProject> projects )
        {
            if( !Initialize( projects ) )
                return false;

            foreach (var compResult in projects.SelectMany(x=>x))
            {
                TraverseInternal(compResult.RootSyntaxNode, compResult);
            }

            return Finalize( projects );
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

        protected virtual bool Finalize( List<CompiledProject> projects )
        {
            Logger?.Information<string>("...finished {0}", Name);

            return _symbolSink.FinalizeSink(this);
        }

        protected abstract bool NodeReferencesSymbol( SyntaxNode node, CompiledFile context, out TSymbol? result );
        protected abstract bool GetChildNodesToVisit( SyntaxNode node, out List<SyntaxNode>? result );

        public bool Equals( IAction<List<CompiledProject>>? other )
            => other switch
            {
                null => false,
                SyntaxWalker<TSymbol> castOther => castOther.SymbolType == SymbolType,
                _ => false
            };

        public bool Equals( ISyntaxWalker? other )
            => other switch
            {
                null => false,
                SyntaxWalker<TSymbol> castOther => castOther.SymbolType == SymbolType,
                _ => false
            };

        bool IAction.Process( object src )
        {
            if( src is List<CompiledProject> projects )
                return Process( projects );

            Logger?.Error( "Expected a {0} but received a {1}", typeof(List<CompiledProject>), src.GetType() );

            return false;
        }
    }
}
