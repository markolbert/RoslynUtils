using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.DocCompiler
{
    public interface IFullyQualifiedNames
    {
        bool Supports( SyntaxNode node );
        bool GetName( SyntaxNode node, out string? result );
        bool GetFullyQualifiedName( SyntaxNode node, out string? result );
    }
}