using System.Text;
using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace J4JSoftware.DocCompiler
{
    public class NamespaceFQN : SyntaxNodeFQN
    {
        public NamespaceFQN(
            IJ4JLogger? logger
        )
            : base( logger, SyntaxKind.NamespaceDeclaration )
        {
        }

        public override bool GetFullyQualifiedName( SyntaxNode node, out string? result )
        {
            result = null;

            if( !base.GetFullyQualifiedName( node, out result ) )
                return false;

            if (!GetName(node, out var startName))
                return false;

            var sb = new StringBuilder(startName!);

            var curNode = node;

            while ((curNode = curNode.Parent) != null && curNode.Kind() == SyntaxKind.NamespaceDeclaration)
            {
                if (!GetName(curNode, out var curName))
                    return false;

                sb.Insert(0, $"{curName}.");
            }

            result = sb.ToString();

            return !string.IsNullOrEmpty(result);
        }
    }
}