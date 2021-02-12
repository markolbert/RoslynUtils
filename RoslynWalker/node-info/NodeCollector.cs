#region license

// Copyright 2021 Mark A. Olbert
// 
// This library or program 'RoslynWalker' is free software: you can redistribute it
// and/or modify it under the terms of the GNU General Public License as
// published by the Free Software Foundation, either version 3 of the License,
// or (at your option) any later version.
// 
// This library or program is distributed in the hope that it will be useful, but
// WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// General Public License for more details.
// 
// You should have received a copy of the GNU General Public License along with
// this library or program.  If not, see <https://www.gnu.org/licenses/>.

#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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

        protected internal Func<ISymbol, SyntaxKind, TSymbol?> SymbolPostProcessor { get; internal set; } =
            ( x, y ) => (TSymbol?) x;

        public NodeCollectors Container { get; }
        public Type SymbolType => typeof(TSymbol);
        public ReadOnlyCollection<TSymbol> Symbols => _symbols.AsReadOnly();

        public bool HandlesNode( SyntaxNode node )
        {
            var kindList = new List<SyntaxKind>();

            var curNode = node;

            do
            {
                kindList.Add( curNode.Kind() );
                curNode = curNode.Parent;
            } while( curNode != null );

            return _nodeKinds.Any( x =>
            {
                var numElements = x.Count;

                if( numElements > kindList.Count )
                    return false;

                for( var idx = numElements - 1; idx >= 0; idx-- )
                    if( x[ idx ] != kindList[ idx ] )
                        return false;

                return true;
            } );
        }

        public void Clear()
        {
            _symbols.Clear();
        }

        public void RemoveDuplicates()
        {
            _symbols = _symbols.Distinct().ToList();
        }

        public StoreSymbolResult StoreSymbol( SyntaxNode node, CompiledFile compiledFile, out ISymbol? result )
        {
            result = null;

            if( !node.GetSymbol<TSymbol>( compiledFile, out var symbol ) )
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

        public IEnumerator<ISymbol> GetEnumerator()
        {
            foreach( var symbol in Symbols ) yield return symbol;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

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

            _nodeKinds.Add( kindList );
            Container.KindFilters.Add( kindList );

            return this;
        }
    }
}