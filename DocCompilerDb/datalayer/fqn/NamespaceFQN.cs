using System.Collections.Generic;
using System.Linq;
using System.Text;
using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace J4JSoftware.DocCompiler
{
    public class NamespaceFQN : FullyQualifiedName
    {
        public NamespaceFQN(
            IJ4JLogger? logger
        )
            : base( logger, SyntaxKind.NamespaceDeclaration )
        {
        }

        public override bool GetFullyQualifiedName( SyntaxNode node, out string? result )
        {
            if( !base.GetFullyQualifiedName( node, out result ) )
                return false;

            if (!GetName(node, out var startName))
                return false;

            var sb = new StringBuilder(startName!);

            var curNode = node.Parent;

            while( curNode?.Kind() == SyntaxKind.NamespaceDeclaration )
            {
                if( !GetName( curNode, out var curName ) )
                    return false;

                sb.Insert( 0, $"{curName}." );

                curNode = curNode.Parent;
            }

            result = sb.ToString();

            return !string.IsNullOrEmpty(result);
        }

        public override bool GetName( SyntaxNode node, out string? result )
        {
            if( !base.GetName( node, out result ) )
                return false;

            if( !GetIdentifierTokens( node, out var idTokens ) )
                return false;

            result = string.Join( ".", idTokens.Select(x=>x.Name) );

            return true;
        }

        public override bool GetIdentifierTokens( SyntaxNode node, out IEnumerable<IIdentifier> result )
        {
            if( !base.GetIdentifierTokens( node, out result ) )
                return false;

            var containerNode = node.ChildNodes()
                .FirstOrDefault( x => x.IsKind( SyntaxKind.QualifiedName ) );

            containerNode ??= node.ChildNodes()
                .FirstOrDefault( x => x.IsKind( SyntaxKind.IdentifierName ) );

            if (containerNode == null)
                return false;

            result = containerNode.DescendantTokens()
                .Where( x => x.IsKind( SyntaxKind.IdentifierToken ) )
                .Select(x=>new BasicIdentifier(x)  );

            return result.Any();
        }
    }
}