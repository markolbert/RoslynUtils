using System.Collections.Generic;
using System.Linq;
using System.Text;
using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace J4JSoftware.DocCompiler
{
    public class UsingFQN : FullyQualifiedName
    {
        public UsingFQN(
            IJ4JLogger? logger
        )
            : base( logger, SyntaxKind.UsingDirective )
        {
        }

        public override bool GetFullyQualifiedName( SyntaxNode node, out string? result )
        {
            return base.GetFullyQualifiedName( node, out result ) 
                   && GetName(node, out result);
        }

        public override bool GetName( SyntaxNode node, out string? result )
        {
            if( !base.GetName( node, out result ) )
                return false;

            if( !GetIdentifierTokens( node, out var idTokens ) )
                return false;

            result = string.Join( ".", idTokens );

            return true;
        }

        public override bool GetIdentifierTokens( SyntaxNode node, out IEnumerable<SyntaxToken> result )
        {
            if( !base.GetIdentifierTokens( node, out result ) )
                return false;

            var containerNode = node.ChildNodes()
                .FirstOrDefault( x => x.IsKind( SyntaxKind.NameEquals ) );

            containerNode ??= node.ChildNodes()
                .FirstOrDefault( x => x.IsKind( SyntaxKind.QualifiedName ) );

            containerNode ??= node.ChildNodes()
                .FirstOrDefault( x => x.IsKind( SyntaxKind.IdentifierName ) );

            if (containerNode == null)
                return false;

            result = containerNode.DescendantTokens()
                .Where( x => x.IsKind( SyntaxKind.IdentifierToken ) );

            return result.Any();
        }
    }
}