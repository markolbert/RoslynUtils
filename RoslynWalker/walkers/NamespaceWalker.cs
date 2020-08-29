using System.Collections.Generic;
using System.Linq;
using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace J4JSoftware.Roslyn.walkers
{
    public class NamespaceWalker : SyntaxWalker<INamespaceSymbol>
    {
        private static readonly List<SyntaxKind> _ignoredNodeKinds = new List<SyntaxKind>();

        static NamespaceWalker()
        {
            _ignoredNodeKinds.Add( SyntaxKind.UsingDirective );
            _ignoredNodeKinds.Add( SyntaxKind.QualifiedName );
        }

        public NamespaceWalker(
            IEnumerable<ISymbolSink> symbolSinks,
            ISymbolInfoFactory symbolInfo,
            IDefaultSymbolSink defaultSymbolSink,
            IJ4JLogger logger
        )
            : base( symbolSinks, defaultSymbolSink, symbolInfo, logger )
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
            {
                Logger.Verbose<string, SyntaxKind>( "{0}: no ISymbol found for node of kind {1}",
                    context.Container.AssemblyName,
                    node.Kind() );

                return false;
            }

            // first check if the symbol is itself an INamespaceSymbol
            if( symbol is INamespaceSymbol nsSymbol )
            {
                result = nsSymbol;
                return true;
            }

            // otherwise, evaluate the symbol's containing namespace
            var containingSymbol = symbol!.ContainingNamespace;

            if(containingSymbol == null )
            {
                Logger.Verbose<string>( "Symbol {0} isn't contained in an Namespace", symbol.ToDisplayString() );

                return false;
            }

            if(containingSymbol.ContainingAssembly == null )
            {
                Logger.Verbose<string>( "Namespace {0} isn't contained in an Assembly", symbol.ToDisplayString() );

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
