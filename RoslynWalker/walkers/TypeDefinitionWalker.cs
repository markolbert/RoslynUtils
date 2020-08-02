using System.Collections.Generic;
using System.Linq;
using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace J4JSoftware.Roslyn.walkers
{
    [ RoslynProcessor( typeof(NamespaceWalker) ) ]
    public class TypeDefinitionWalker : SyntaxWalker<INamedTypeSymbol>
    {
        private static readonly List<SyntaxKind> _ignoredNodeKinds = new List<SyntaxKind>();

        static TypeDefinitionWalker()
        {
            _ignoredNodeKinds.Add( SyntaxKind.UsingDirective );
            _ignoredNodeKinds.Add( SyntaxKind.QualifiedName );
            //_ignoredNodeKinds.Add( SyntaxKind.TypeParameter );
            //_ignoredNodeKinds.Add( SyntaxKind.TypeParameterConstraintClause );
            //_ignoredNodeKinds.Add( SyntaxKind.ParameterList );
        }

        public TypeDefinitionWalker(
            IEnumerable<ISymbolSink> symbolSinks,
            ISymbolName symbolName,
            IDefaultSymbolSink defaultSymbolSink,
            IJ4JLogger logger
        )
            : base( symbolSinks, defaultSymbolSink, symbolName, logger )
        {
        }

        protected override bool NodeReferencesSymbol( SyntaxNode node, 
            CompiledFile context,
            out INamedTypeSymbol? result )
        {
            result = null;

            // certain node types don't lead to places we need to process
            if( _ignoredNodeKinds.Any( nk => nk == node.Kind() ) )
                return false;

            if( !context.GetSymbol<INamedTypeSymbol>( node, out var typeSymbol ) )
            {
                Logger.Verbose<string, SyntaxKind>( "{0}: no INamedTypeSymbol found for node of kind {1}",
                    context.Container.AssemblyName,
                    node.Kind() );

                return false;
            }

            result = typeSymbol;

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
