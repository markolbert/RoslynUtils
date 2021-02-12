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
using System.Diagnostics;
using System.Linq;
using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace J4JSoftware.Roslyn
{
    public class SyntaxWalkerNG : ISyntaxWalkerNG
    {
        private readonly List<SyntaxKind> _alwaysIgnore;
        private readonly List<SyntaxKind> _ignoreAfterProcessing;
        private readonly IJ4JLogger? _logger;
        private readonly List<SyntaxKind> _passThru;
        private readonly List<SyntaxNode> _visitedNodes = new();

        public SyntaxWalkerNG(
            IJ4JLogger? logger
        )
        {
            _logger = logger;
            _logger?.SetLoggedType( GetType() );

            // set up the collection of nodes/symbols we're scanning for
            // IAssemblySymbols, INamespaceSymbols and attribute stuff can't be derived directly
            // from their SyntaxNodes. They are discovered indirectly as a result
            // of their association with other symbols.
            var junk = NodeCollectors.Add<IEventSymbol>()
                .MapsNodeStack( SyntaxKind.VariableDeclarator,
                    SyntaxKind.VariableDeclaration,
                    SyntaxKind.EventFieldDeclaration )
                .MapsNodeStack( SyntaxKind.EventDeclaration,
                    SyntaxKind.ClassDeclaration );

            NodeCollectors.Add<IFieldSymbol>()
                .MapsNodeStack( SyntaxKind.VariableDeclarator,
                    SyntaxKind.VariableDeclaration,
                    SyntaxKind.FieldDeclaration );

            NodeCollectors.Add<IMethodSymbol>()
                .MapsNodeStack( SyntaxKind.MethodDeclaration )
                .MapsNodeStack( SyntaxKind.ConstructorDeclaration );

            NodeCollectors.Add<IPropertySymbol>()
                .MapsNodeStack( SyntaxKind.PropertyDeclaration )
                .MapsNodeStack( SyntaxKind.IndexerDeclaration );

            NodeCollectors.Add<IParameterSymbol>()
                .MapsNodeStack( SyntaxKind.Parameter );

            NodeCollectors.Add( TypePostProcessor )
                .MapsNodeStack( SyntaxKind.ClassDeclaration )
                .MapsNodeStack( SyntaxKind.DelegateDeclaration )
                .MapsNodeStack( SyntaxKind.InterfaceDeclaration )
                .MapsNodeStack( SyntaxKind.IdentifierName, SyntaxKind.TypeConstraint );

            NodeCollectors.Add<ITypeParameterSymbol>()
                .MapsNodeStack( SyntaxKind.TypeParameter )
                .MapsNodeStack( SyntaxKind.IdentifierName, SyntaxKind.TypeArgumentList, SyntaxKind.GenericName )
                .MapsNodeStack( SyntaxKind.IdentifierName, SyntaxKind.TypeParameterConstraintClause )
                .MapsNodeStack( SyntaxKind.IdentifierName, SyntaxKind.MethodDeclaration )
                .MapsNodeStack( SyntaxKind.IdentifierName, SyntaxKind.ArrayType );

            _alwaysIgnore = new List<SyntaxKind>
            {
                SyntaxKind.BaseList,
                SyntaxKind.Block,
                SyntaxKind.PredefinedType,
                SyntaxKind.QualifiedName,
                SyntaxKind.UsingDirective
            };

            _ignoreAfterProcessing = new List<SyntaxKind>
            {
                SyntaxKind.AttributeList,
                SyntaxKind.EventDeclaration,
                SyntaxKind.EventFieldDeclaration,
                SyntaxKind.FieldDeclaration,
                SyntaxKind.IndexerDeclaration,
                SyntaxKind.MethodDeclaration,
                SyntaxKind.Parameter,
                SyntaxKind.PropertyDeclaration,
                SyntaxKind.TypeParameter,
                SyntaxKind.VariableDeclarator
            };

            _passThru = new List<SyntaxKind>
            {
                SyntaxKind.CompilationUnit,
                SyntaxKind.GenericName,
                SyntaxKind.NamespaceDeclaration,
                SyntaxKind.ParameterList,
                SyntaxKind.TypeArgumentList,
                SyntaxKind.TypeParameterList,
                SyntaxKind.VariableDeclaration
            };
        }

        public NodeCollectors NodeCollectors { get; } = new();

        public void Process( List<CompiledProject> projects )
        {
            _logger?.Information( "Starting SyntaxWalker..." );

            NodeCollectors.Initialize( projects );
            _visitedNodes.Clear();

            foreach( var compiledFile in projects.SelectMany( x => x ) )
                TraverseInternal( compiledFile.RootSyntaxNode, compiledFile );

            _logger?.Information( "...finished" );

            NodeCollectors.RemoveDuplicates();
        }

        private void TraverseInternal( SyntaxNode node, CompiledFile compiledFile )
        {
            var nodeKind = node.Kind();

            // this should never get tripped, but...
            if( _alwaysIgnore.Any( x => x == nodeKind ) )
            {
                CheckSymbolMapping( node, compiledFile, null );
                return;
            }

            // don't re-visit nodes
            if( _visitedNodes.Any( vn => vn.Equals( node ) ) )
            {
                CheckSymbolMapping( node, compiledFile, null );
                return;
            }

            _visitedNodes.Add( node );

            ISymbol? mappedSymbol = null;

            // don't try to map node types that are passthru
            if( _passThru.All( x => x != nodeKind ) )
            {
                var mapper = NodeCollectors[ node ];

                if( mapper == null )
                {
                    _logger?.Warning( "No symbol mapper for {0} node", nodeKind );
                }
                else
                {
                    switch( mapper.StoreSymbol( node, compiledFile, out var temp ) )
                    {
                        case StoreSymbolResult.NotInDocumentationScope:
                            // don't drill outside documentation scope
                            return;

                        case StoreSymbolResult.NotFound:
                            // ITypeParameter IdentifierName nodes are important, the rest, not so much...
                            if( nodeKind != SyntaxKind.IdentifierName )
                                _logger?.Warning( "Failed to map {0} SyntaxNode to an ISymbol", nodeKind );

                            break;
                    }

                    mappedSymbol = temp;
                }

                CheckSymbolMapping( node, compiledFile, mappedSymbol );
            }

            foreach( var childNode in GetChildNodesToVisit( node ) ) TraverseInternal( childNode, compiledFile );
        }

        [ Conditional( "DEBUG" ) ]
        private void CheckSymbolMapping( SyntaxNode node, CompiledFile compiledFile, ISymbol? mappedSymbol )
        {
            var audited = new FoundSymbol( node, compiledFile );

            var auditSymbol = audited.Symbol == null || !NodeCollectors.InDocumentationScope( audited.Symbol )
                ? null
                : audited.Symbol;

            if( mappedSymbol == null && auditSymbol == null
                || mappedSymbol != null && auditSymbol != null
                                        && SymbolEqualityComparer.Default
                                            .Equals( mappedSymbol, auditSymbol ) )
                return;

            _logger?.Debug<string, string>( "Mapping routine found {0}, audit routine found {1}",
                mappedSymbol?.GetType().Name ?? "** no symbol mapped **",
                audited.ToString() );
        }

        private IEnumerable<SyntaxNode> GetChildNodesToVisit( SyntaxNode node )
        {
            var nodeKind = node.Kind();

            if( _alwaysIgnore.Any( x => x == nodeKind )
                || _ignoreAfterProcessing.Any( x => x == nodeKind ) )
                return Enumerable.Empty<SyntaxNode>();

            return node.Kind() switch
            {
                SyntaxKind.MethodKeyword => node.ChildNodes().Where( x =>
                {
                    var childKind = x.Kind();

                    return _alwaysIgnore.All( y => y != childKind )
                           && childKind != SyntaxKind.SimpleLambdaExpression
                           && childKind != SyntaxKind.ParenthesizedLambdaExpression
                           && childKind != SyntaxKind.GetAccessorDeclaration
                           && childKind != SyntaxKind.SetAccessorDeclaration;
                } ),
                _ => node.ChildNodes().Where( x => _alwaysIgnore.All( y => y != x.Kind() ) )
            };
        }

        private ITypeSymbol? TypePostProcessor( ISymbol symbol, SyntaxKind nodeKind )
        {
            // this oddball test is to ensure we capture System.Void types, which are the
            // "return types" of >>all<< constructors but aren't represented by their own
            // SyntaxNode. Consequently, unless the source code contains a method with a return
            // type of void it's possible for System.Void to be overlooked, causing problems
            // down the road.
            if( nodeKind == SyntaxKind.ConstructorDeclaration )
                return ( (IMethodSymbol) symbol ).ReturnType;

            return (ITypeSymbol) symbol;
        }
    }
}