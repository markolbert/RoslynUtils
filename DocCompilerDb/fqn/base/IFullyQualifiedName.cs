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
        Type SupportedType { get; }
        bool GetName( object source, out string? result );
        bool GetFullyQualifiedName( object source, out string? result );
    }

    public interface IFullyQualifiedName<in TSource> : IFullyQualifiedName
    {
        bool GetName( TSource source, out string? result );
        bool GetFullyQualifiedName( TSource source, out string? result );
    }

    public interface IFullyQualifiedNameSyntaxNode : IFullyQualifiedName<SyntaxNode>
    {
        ReadOnlyCollection<SyntaxKind> SupportedKinds { get; }
        bool GetIdentifierTokens( SyntaxNode node, out IEnumerable<SyntaxNode>? result );
    }
}
