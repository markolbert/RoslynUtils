using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace J4JSoftware.DocCompiler
{
    public interface IFullyQualifiedName
    {
        ReadOnlyCollection<SyntaxKind> SupportedKinds { get; }
        bool GetIdentifierTokens( SyntaxNode node, out IEnumerable<SyntaxToken> result );
        bool GetName( SyntaxNode node, out string? result );
        bool GetFullyQualifiedName( SyntaxNode node, out string? result );
    }
}
