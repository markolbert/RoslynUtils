using System.Collections.Generic;
using System.Linq;
using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace J4JSoftware.Roslyn.walkers
{
    public class TypeWalker : SyntaxWalker<ITypeSymbol>
    {
        private static readonly List<SyntaxKind> _ignoredNodeKinds = new List<SyntaxKind>();

        static TypeWalker()
        {
            _ignoredNodeKinds.Add( SyntaxKind.UsingDirective );
            _ignoredNodeKinds.Add( SyntaxKind.QualifiedName );
            //_ignoredNodeKinds.Add( SyntaxKind.TypeParameter );
            //_ignoredNodeKinds.Add( SyntaxKind.TypeParameterConstraintClause );
            //_ignoredNodeKinds.Add( SyntaxKind.ParameterList );
        }

        public TypeWalker(
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
                && context.GetSymbol<IMethodSymbol>( node, out var methodSymbol )
                /* && methodSymbol!.ReturnType is INamedTypeSymbol returnSymbol */ )
            {
                result = methodSymbol!.ReturnType;
                return true;
            }

            if( !context.GetSymbol<ITypeSymbol>( node, out var typeSymbol ) )
            {
                Logger.Verbose<string, SyntaxKind>( "{0}: no ITypeSymbol found for node of kind {1}",
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
