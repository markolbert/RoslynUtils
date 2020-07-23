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
        private readonly List<SyntaxNode> _visitedNodes = new List<SyntaxNode>();
        private readonly List<string> _procSymbolNames = new List<string>();

        protected SyntaxWalker(
            IEnumerable<ISymbolSink> symbolSinks,
            IDefaultSymbolSink defaultSymbolSink,
            SymbolNamers symbolNamers,
            IJ4JLogger logger
        )
        {
            Logger = logger;
            Logger.SetLoggedType( this.GetType() );

            SymbolNamers = symbolNamers;
            SymbolType = typeof(TTarget);

            var sink = symbolSinks.FirstOrDefault( s => s.SupportsSymbol( SymbolType ) && !( s is IDefaultSymbolSink ) );

            _symbolSink = sink ?? defaultSymbolSink;
        }

        protected IJ4JLogger Logger { get; }

        public Type SymbolType { get; }

        public ReadOnlyCollection<IAssemblySymbol> ModelAssemblies => _modelAssemblies.AsReadOnly();

        protected SymbolNamers SymbolNamers { get; }

        public virtual bool Traverse( List<CompiledProject> compResults )
        {
            _modelAssemblies.Clear();
            _modelAssemblies.AddRange( compResults.Select( cr => cr.AssemblySymbol ).Distinct() );

            _visitedNodes.Clear();
            _procSymbolNames.Clear();

            _symbolSink.InitializeSink();

            foreach( var compResult in compResults.SelectMany(cr=>cr) )
            {
                TraverseInternal( compResult.RootSyntaxNode, compResult );
            }

            return true;
        }

        protected void TraverseInternal( SyntaxNode node, CompiledFile context )
        {
            // don't re-visit nodes
            if( _visitedNodes.Any( vn => vn.Equals( node ) ) )
                return;

            _visitedNodes.Add( node );

            if( ShouldSinkNodeSymbol( node, context, out var symbol ) )
                _symbolSink?.OutputSymbol( symbol! );

            if( !GetChildNodesToVisit( node, out var children ) )
                return;

            foreach( var childNode in children! )
            {
                TraverseInternal( childNode, context );
            }
        }

        protected abstract bool ShouldSinkNodeSymbol( SyntaxNode node, CompiledFile context, out TTarget? result );
        protected abstract bool GetChildNodesToVisit( SyntaxNode node, out List<SyntaxNode>? result );

        protected bool AssemblyInScope( IAssemblySymbol toCheck ) 
            => ModelAssemblies.Any( ma => SymbolEqualityComparer.Default.Equals(ma, toCheck));

        protected virtual bool SymbolIsUnProcessed(TTarget symbol)
        {
            var otherName = SymbolNamers.GetSymbolName( symbol );

            if (_procSymbolNames.Any(psn => psn.Equals(otherName, StringComparison.OrdinalIgnoreCase)))
            {
                Logger.Verbose<Type, TTarget>("{0} '{1}' was already processed", typeof(TTarget), symbol);

                return false;
            }

            _procSymbolNames.Add(otherName);

            return true;
        }

        public bool Equals( ISyntaxWalker? other )
        {
            if( other == null )
                return false;

            return other.SymbolType == SymbolType;
        }
    }
}
