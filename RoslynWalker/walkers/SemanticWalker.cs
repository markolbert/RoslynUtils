using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn
{
    public abstract class SemanticWalker<TTarget> : ISemanticWalker
        where TTarget : class, ISymbol
    {
        private readonly ISymbolSink? _symbolSink;

        protected SemanticWalker(
            IEnumerable<ISymbolSink> symbolSinks,
            IDefaultSymbolSink? defaultSymbolSink,
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

        public List<IAssemblySymbol> ModelAssemblies { get; } = new List<IAssemblySymbol>();

        protected List<SyntaxNode> VisitedNodes { get; } = new List<SyntaxNode>();

        public bool Traverse( List<CompilationResults> compResults, Action<TTarget> symbolProcessor )
        {
            ModelAssemblies.Clear();
            ModelAssemblies.AddRange( compResults.Select( cr => cr.AssemblySymbol ).Distinct() );

            VisitedNodes.Clear();

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
                _symbolSink?.OutputSymbol( symbol! );

            foreach( var childNode in GetTraversableChildren( node ) )
            {
                TraverseInternal(childNode, context);
            }
        }

        protected abstract bool ProcessNode( SyntaxNode node, CompilationResult context, out TTarget? result );
        protected abstract List<SyntaxNode> GetTraversableChildren( SyntaxNode node );

        protected bool AssemblyInScope( IAssemblySymbol toCheck ) 
            => ModelAssemblies.Any( ma => SymbolEqualityComparer.Default.Equals(ma, toCheck));

        public bool Equals( ISemanticWalker? other )
        {
            if( other == null )
                return false;

            return other.SymbolType == SymbolType;
        }
    }
}
