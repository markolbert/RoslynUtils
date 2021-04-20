using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.DocCompiler
{
    public interface IFullyQualifiedNodeNames
    {
        bool GetName( SyntaxNode node, 
            out List<string>? result, 
            bool includeTypeParams = true );
    }
}