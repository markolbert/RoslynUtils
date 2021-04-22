using Microsoft.CodeAnalysis;

namespace J4JSoftware.DocCompiler
{
    public interface IFullyQualifiedNodeNames
    {
        ResolvedNameState GetName( SyntaxNode node,
            out string? result, 
            bool includeTypeParams = true );
    }
}