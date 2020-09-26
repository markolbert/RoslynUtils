using System.Collections.Generic;
using System.Linq;
using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace J4JSoftware.Roslyn
{
    public class TypeWalker : SyntaxWalker<ITypeSymbol>
    {
        private static readonly List<SyntaxKind> _ignoredNodeKinds = new List<SyntaxKind>();

        static TypeWalker()
        {
            _ignoredNodeKinds.Add( SyntaxKind.UsingDirective );
            _ignoredNodeKinds.Add( SyntaxKind.QualifiedName );
        }

        public TypeWalker(
            ISymbolFullName symbolInfo,
            IDefaultSymbolSink defaultSymbolSink,
            ExecutionContext context,
            IJ4JLogger logger,
            ISymbolSink<ITypeSymbol>? symbolSink = null
        )
            : base( symbolInfo, defaultSymbolSink, context, logger, symbolSink )
        {
        }

        protected override bool NodeReferencesSymbol( SyntaxNode node, 
            CompiledFile context,
            out ITypeSymbol? result )
        {
            result = null;

            // certain node types don't lead to places we need to process
            if( _ignoredNodeKinds.Any( nk => nk == node.Kind() ) )
                return false;

            // this oddball test is to ensure we capture System.Void types, which are the
            // "return types" of >>all<< constructors but aren't represented by their own
            // SyntaxNode. Consequently, unless the source code contains a method with a return
            // type of void it's possible for System.Void to be overlooked, causing problems
            // down the road.
            if( node.Kind() == SyntaxKind.ConstructorDeclaration
                && context.GetSymbol<IMethodSymbol>( node, out var methodSymbol ) )
            {
                result = methodSymbol!.ReturnType;
                return true;
            }

            if( !context.GetSymbol<ITypeSymbol>( node, out var typeSymbol ) )
                return false;

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
