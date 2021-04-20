using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace J4JSoftware.DocCompiler
{
    public interface IFullyQualifiedNodeName
    {
        ReadOnlyCollection<SyntaxKind> SupportedKinds { get; }
        bool GetIdentifierTokens( SyntaxNode node, out IEnumerable<IIdentifier> result );
        bool GetName( SyntaxNode node, out string? result );
        bool GetFullyQualifiedName( SyntaxNode node, out string? result );
    }
}
