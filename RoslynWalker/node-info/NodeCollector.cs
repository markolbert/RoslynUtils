using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace J4JSoftware.Roslyn
{
    public enum StoreSymbolResult
    {
        NotFound,
        NotInDocumentationScope,
        Stored
    }

    public class NodeCollector<TSymbol> : INodeCollector<TSymbol>
        where TSymbol : class, ISymbol
    {
        private readonly List<List<SyntaxKind>> _nodeKinds = new();

        private List<TSymbol> _symbols = new();

        internal NodeCollector( NodeCollectors container )
        {
            Container = container;
        }

        public NodeCollectors Container { get; }
        public Type SymbolType => typeof(TSymbol);
        public ReadOnlyCollection<TSymbol> Symbols => _symbols.AsReadOnly();

        // specify the node kinds from innermost to outermost
        public NodeCollector<TSymbol> MapsNodeStack( params SyntaxKind[] nodeKinds )
        {
            // check for duplicates
            if( Container.KindFilters.Any( x =>
            {
                var identical = true;

                for( var idx = 0; idx < x.Count; idx++ )
                {
                    if( idx >= nodeKinds.Length )
                        break;

                    identical &= x[ idx ] == nodeKinds[ idx ];
                }

                return identical;
            } ) )
                throw new ArgumentException( $"Overlapping node stack ({string.Join( ", ", nodeKinds )})" );

            var kindList = nodeKinds.ToList();
            
            _nodeKinds.Add(kindList  );
            Container.KindFilters.Add( kindList );

            return this;
        }

        public bool HandlesNode( SyntaxNode node )
        {
            var kindList = new List<SyntaxKind>();

            SyntaxNode? curNode = node;

            do
            {
                kindList.Add(curNode.Kind()  );
                curNode = curNode.Parent;
            } while( curNode != null );

            return _nodeKinds.Any( x =>
            {
                var numElements = x.Count;

                if( numElements > kindList.Count )
                    return false;

                for( var idx = numElements - 1; idx >= 0; idx-- )
                {
                    if( x[idx]!= kindList[idx])
                        return false;
                }

                return true;
            } );
        }

        public void Clear() =>_symbols.Clear();
        public void RemoveDuplicates() =>_symbols = _symbols.Distinct().ToList();

        public StoreSymbolResult StoreSymbol( SyntaxNode node, CompiledFile compiledFile, out ISymbol? result )
        {
            result = null;

            if( !node.GetSymbol<TSymbol>(compiledFile, out var symbol ) )
                return StoreSymbolResult.NotFound;

            var toAdd = SymbolPostProcessor( symbol!, node.Kind() );
            if( toAdd == null )
                return StoreSymbolResult.NotFound;

            if( !Container.InDocumentationScope( toAdd ) )
                return StoreSymbolResult.NotInDocumentationScope;

            _symbols.Add( toAdd );

            Container.StoreAssemblyNamespace( toAdd );

            result = toAdd;

            return StoreSymbolResult.Stored;
        }

        protected internal Func<ISymbol, SyntaxKind, TSymbol?> SymbolPostProcessor { get; internal set; } = ( x, y ) => (TSymbol?) x;

        public IEnumerator<ISymbol> GetEnumerator()
        {
            foreach( var symbol in Symbols )
            {
                yield return symbol;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}