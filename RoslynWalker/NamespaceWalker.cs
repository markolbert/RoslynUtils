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

using System.Collections.Generic;
using System.Linq;
using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace J4JSoftware.Roslyn
{
    public class NamespaceWalker : SyntaxWalker<INamespaceSymbol>
    {
        private static readonly List<SyntaxKind> _ignoredNodeKinds = new();

        static NamespaceWalker()
        {
            _ignoredNodeKinds.Add( SyntaxKind.UsingDirective );
            _ignoredNodeKinds.Add( SyntaxKind.QualifiedName );
        }

        public NamespaceWalker(
            ISymbolFullName symbolInfo,
            IDefaultSymbolSink defaultSymbolSink,
            WalkerContext context,
            IJ4JLogger? logger,
            ISymbolSink<INamespaceSymbol>? symbolSink = null
        )
            : base( "Namespace walking", symbolInfo, defaultSymbolSink, context, logger, symbolSink )
        {
        }

        protected override bool NodeReferencesSymbol( SyntaxNode node, CompiledFile context,
            out INamespaceSymbol? result )
        {
            result = null;

            // certain node types don't lead to places we need to process
            if( _ignoredNodeKinds.Any( nk => nk == node.Kind() ) )
                return false;

            if( !context.GetSymbol<ISymbol>( node, out var symbol ) )
                return false;

            // first check if the symbol is itself an INamespaceSymbol
            if( symbol is INamespaceSymbol nsSymbol )
            {
                result = nsSymbol;
                return true;
            }

            // otherwise, evaluate the symbol's containing namespace
            var containingSymbol = symbol!.ContainingNamespace;

            if( containingSymbol == null )
            {
                Logger?.Verbose<string>( "Symbol {0} isn't contained in an Namespace", symbol.ToDisplayString() );

                return false;
            }

            if( containingSymbol.ContainingAssembly == null )
            {
                Logger?.Verbose<string>( "Namespace {0} isn't contained in an Assembly", symbol.ToDisplayString() );

                return false;
            }

            result = containingSymbol;

            return true;
        }

        protected override bool GetChildNodesToVisit( SyntaxNode node, out List<SyntaxNode>? result )
        {
            result = null;

            // we're interested in traversing almost everything that's within scope
            // except for node types that we know don't lead any place interesting
            if( _ignoredNodeKinds.Any( nk => nk == node.Kind() ) )
                return false;

            result = node.ChildNodes()
                .Where( n => _ignoredNodeKinds.All( i => i != n.Kind() ) )
                .ToList();

            return true;
        }
    }
}