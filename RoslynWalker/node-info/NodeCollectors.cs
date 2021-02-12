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
    public class NodeCollectors : IEnumerable<INodeCollector>
    {
        private readonly List<IAssemblySymbol> _assemblies = new();
        private readonly AttributeListCollector _attributeCollector;
        private readonly List<AttributeNodeInfo> _attributes = new();
        private readonly List<INodeCollector> _children = new();
        private readonly List<INamespaceSymbol> _namespaces = new();

        private List<IAssemblySymbol>? _projAssemblies;

        public NodeCollectors()
        {
            _attributeCollector = new AttributeListCollector( this );
        }

        internal List<List<SyntaxKind>> KindFilters { get; } = new();

        public bool IsInitialized => _projAssemblies != null;

        public ReadOnlyCollection<INodeCollector> Children => _children.AsReadOnly();
        public ReadOnlyCollection<IAssemblySymbol> Assemblies => _assemblies.AsReadOnly();
        public ReadOnlyCollection<INamespaceSymbol> Namespaces => _namespaces.AsReadOnly();
        public ReadOnlyCollection<AttributeNodeInfo> Attributes => _attributes.AsReadOnly();

        public INodeCollectorBase? this[ SyntaxNode node ]
        {
            get
            {
                if( node.IsKind( SyntaxKind.AttributeList ) )
                    return _attributeCollector;

                return _children.FirstOrDefault( x => x.HandlesNode( node ) );
            }
        }

        public IEnumerator<INodeCollector> GetEnumerator()
        {
            foreach( var child in _children ) yield return child;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Initialize( List<CompiledProject> projects )
        {
            _projAssemblies = projects.Select( cp => cp.AssemblySymbol )
                .Distinct()
                .ToList();

            foreach( var nodeToSymbol in this ) nodeToSymbol.Clear();

            KindFilters.Clear();
        }

        public bool InDocumentationScope( ISymbol toCheck )
        {
            return _projAssemblies?
                       .Any( x => SymbolEqualityComparer.Default.Equals( x, toCheck.ContainingAssembly ) )
                   ?? false;
        }

        public NodeCollector<TSymbol> Add<TSymbol>( Func<ISymbol, SyntaxKind, TSymbol?>? symbolPostProcessor = null )
            where TSymbol : class, ISymbol
        {
            var retVal = new NodeCollector<TSymbol>( this );
            _children.Add( retVal );

            if( symbolPostProcessor != null )
                retVal.SymbolPostProcessor = symbolPostProcessor;

            return retVal;
        }

        public void StoreAssemblyNamespace( ISymbol symbol )
        {
            if( symbol.ContainingAssembly != null )
                _assemblies.Add( symbol.ContainingAssembly );

            if( symbol.ContainingNamespace != null )
                _namespaces.Add( symbol.ContainingNamespace );
        }

        public void StoreAttributeList( SyntaxNode node, ISymbol? attributedSymbol, CompiledFile compiledFile )
        {
            if( node.Kind() != SyntaxKind.AttributeList )
                return;

            if( attributedSymbol == null && node.Parent?.Kind() != SyntaxKind.CompilationUnit )
                return;

            _attributes.Add( new AttributeNodeInfo( node, attributedSymbol, compiledFile ) );
        }

        public void RemoveDuplicates()
        {
            foreach( var child in _children ) child.RemoveDuplicates();
        }
    }
}