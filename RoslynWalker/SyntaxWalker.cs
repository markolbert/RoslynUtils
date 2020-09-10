using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn
{
    public abstract class SyntaxWalker<TTarget> : ISyntaxWalker
        where TTarget : class, ISymbol
    {
        private readonly ISymbolSink _symbolSink;
        private readonly List<IAssemblySymbol> _modelAssemblies = new List<IAssemblySymbol>();
        private readonly List<SyntaxNode> _visitedNodes = new List<SyntaxNode>();

        protected SyntaxWalker(
            IEnumerable<ISymbolSink> symbolSinks,
            IDefaultSymbolSink defaultSymbolSink,
            ISymbolFullName symbolInfo,
            IJ4JLogger logger
        )
        {
            Logger = logger;
            Logger.SetLoggedType( this.GetType() );

            SymbolInfo = symbolInfo;
            SymbolType = typeof(TTarget);

            var sink = symbolSinks.FirstOrDefault( s => s.SupportsSymbol( SymbolType ) && !( s is IDefaultSymbolSink ) );

            if( sink == null )
                Logger.Error<Type>( "Couldn't find an ISymbolSink for type {0}", SymbolType );

            _symbolSink = sink ?? defaultSymbolSink;
        }

        protected IJ4JLogger Logger { get; }
        protected ISymbolFullName SymbolInfo { get; }

        public ISyntaxWalker? Predecessor { get; set; }
        public Type SymbolType { get; }

        public ReadOnlyCollection<IAssemblySymbol> DocumentationAssemblies => _modelAssemblies.AsReadOnly();

        public bool InDocumentationScope(IAssemblySymbol toCheck)
            => DocumentationAssemblies.Any(ma => SymbolEqualityComparer.Default.Equals(ma, toCheck));

        public virtual bool Process( List<CompiledProject> compResults, bool stopOnFirstError = false )
        {
            _modelAssemblies.Clear();
            _modelAssemblies.AddRange( compResults.Select( cr => cr.AssemblySymbol ).Distinct() );

            _visitedNodes.Clear();

            if( !_symbolSink.InitializeSink(this, stopOnFirstError) )
                return false;

            foreach( var compResult in compResults.SelectMany(cr=>cr) )
            {
                TraverseInternal( compResult.RootSyntaxNode, compResult );
            }

            return _symbolSink.FinalizeSink(this);
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

        protected abstract bool NodeReferencesSymbol( SyntaxNode node, CompiledFile context, out TTarget? result );
        protected abstract bool GetChildNodesToVisit( SyntaxNode node, out List<SyntaxNode>? result );

        public bool Equals( ISyntaxWalker? other )
        {
            if( other == null )
                return false;

            return other.SymbolType == SymbolType;
        }
    }
}
