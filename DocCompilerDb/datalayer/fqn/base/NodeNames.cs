using System.Collections.Generic;
using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace J4JSoftware.DocCompiler
{
    public class NodeNames : NodeNamesBase, INodeNames
    {
        public NodeNames(
            INodeIdentifierTokens idTokens,
            DocDbContext dbContext,
            IJ4JLogger? logger
        )
        :base(idTokens, dbContext, logger)
        {
        }

        public bool GetName( SyntaxNode node, out string? result, bool includeTypeParams = true )
        {
            IncludeTypeParameters = includeTypeParams;

            return GetNameInternal( node, out result );
        }
    }
}
