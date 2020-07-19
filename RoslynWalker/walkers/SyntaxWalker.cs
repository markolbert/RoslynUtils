using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn
{
    public abstract class SyntaxWalker<TTarget> : ISyntaxWalker<TTarget>
        where TTarget : class, ISymbol
    {
        private readonly ISymbolSink _symbolSink;
        private readonly List<IAssemblySymbol> _modelAssemblies = new List<IAssemblySymbol>();

        protected SyntaxWalker(
            IEnumerable<ISymbolSink> symbolSinks,
            IDefaultSymbolSink defaultSymbolSink,
            IJ4JLogger logger
        )
        {
            Logger = logger;
            Logger.SetLoggedType( this.GetType() );

            SymbolType = typeof(TTarget);

            var sink = symbolSinks.FirstOrDefault( s => s.SupportsSymbol( SymbolType ) && !( s is IDefaultSymbolSink ) );

            _symbolSink = sink ?? defaultSymbolSink;
        }

        protected IJ4JLogger Logger { get; }

        public Type SymbolType { get; }

        public ReadOnlyCollection<IAssemblySymbol> ModelAssemblies => _modelAssemblies.AsReadOnly();

        protected List<SyntaxNode> VisitedNodes { get; } = new List<SyntaxNode>();
        protected List<TTarget> ProcessedSymbols { get; } = new List<TTarget>();

        public virtual bool Traverse( List<CompilationResults> compResults )
        {
            _modelAssemblies.Clear();
            _modelAssemblies.AddRange( compResults.Select( cr => cr.AssemblySymbol ).Distinct() );

            ProcessedSymbols.Clear();

            VisitedNodes.Clear();

            _symbolSink.InitializeSink();

            foreach( var compResult in compResults.SelectMany(cr=>cr) )
            {
                TraverseInternal( compResult.RootSyntaxNode, compResult );
            }

            return true;
        }

        protected void TraverseInternal( SyntaxNode node, CompilationResult context )
        {
            // don't re-visit nodes
            if( VisitedNodes.Any( vn => vn.Equals( node ) ) )
                return;

            VisitedNodes.Add( node );

            if( ProcessNode( node, context, out var symbol ) )
            {
                _symbolSink?.OutputSymbol(symbol!);

                // keep track of the symbols we've processed
                if ( !ProcessedSymbols.Any( ps => SymbolEqualityComparer.Default.Equals(ps, symbol)) )
                    ProcessedSymbols.Add( symbol! );
            }

            if( !GetTraversableChildren( node, out var children ) ) 
                return;

            foreach( var childNode in children! )
            {
                TraverseInternal( childNode, context );
            }
        }

        protected abstract bool ProcessNode( SyntaxNode node, CompilationResult context, out TTarget? result );
        protected abstract bool GetTraversableChildren( SyntaxNode node, out List<SyntaxNode>? result );

        protected bool AssemblyInScope( IAssemblySymbol toCheck ) 
            => ModelAssemblies.Any( ma => SymbolEqualityComparer.Default.Equals(ma, toCheck));

        public bool Equals( ISyntaxWalker? other )
        {
            if( other == null )
                return false;

            return other.SymbolType == SymbolType;
        }
    }
}
