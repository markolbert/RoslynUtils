using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.DocCompiler
{
    public class NodeNames : NodeNamesBase, INodeNames
    {
        public NodeNames(
            INodeIdentifierTokens idTokens,
            IJ4JLogger? logger
        )
            : base( idTokens, logger )
        {
        }

        public ResolvedNameState GetName( SyntaxNode node, out string? result, bool includeTypeParams = true )
        {
            IncludeTypeParameters = includeTypeParams;

            return GetNameInternal( node, out result );
        }
    }
}
