using System.Linq;
using System.Text;
using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace J4JSoftware.DocCompiler
{
    public class TypeParameterListFQN : SyntaxNodeFQN
    {
        public TypeParameterListFQN(
            IJ4JLogger? logger
        )
            : base( logger, SyntaxKind.TypeParameterList )
        {
        }

        public override bool GetFullyQualifiedName( SyntaxNode node, out string? result )
        {
            result = null;

            if( !base.GetFullyQualifiedName( node, out result ) )
                return false;

            if (!GetIdentifierTokens(node, out var idTokens))
                return false;

            result = $"<{string.Join( ", ", idTokens!)}>";

            return true;
        }
    }
}