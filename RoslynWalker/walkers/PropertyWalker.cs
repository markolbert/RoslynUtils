using System.Collections.Generic;
using System.Linq;
using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace J4JSoftware.Roslyn.walkers
{
    public class PropertyWalker : SyntaxWalker<IPropertySymbol>
    {
        private static readonly List<SyntaxKind> _ignoredNodeKinds = new List<SyntaxKind>();

        static PropertyWalker()
        {
            _ignoredNodeKinds.Add( SyntaxKind.UsingDirective );
            _ignoredNodeKinds.Add( SyntaxKind.QualifiedName );
        }

        public PropertyWalker(
            IEnumerable<ISymbolSink> symbolSinks,
            ISymbolNamer symbolInfo,
            IDefaultSymbolSink defaultSymbolSink,
            IJ4JLogger logger
        )
            : base( symbolSinks, defaultSymbolSink, symbolInfo, logger )
        {
        }

        protected override bool NodeReferencesSymbol( SyntaxNode node, 
            CompiledFile context,
            out IPropertySymbol? result )
        {
            result = null;

            // certain node types don't lead to places we need to process
            if( _ignoredNodeKinds.Any( nk => nk == node.Kind() ) )
                return false;

            if( !context.GetSymbol<IPropertySymbol>( node, out var typeSymbol ) )
            {
                Logger.Verbose<string, SyntaxKind>( "{0}: no IPropertySymbol found for node of kind {1}",
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
