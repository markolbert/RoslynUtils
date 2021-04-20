using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace J4JSoftware.DocCompiler
{
    public record NodeContext( SyntaxNode Node, IScannedFile ScannedFile )
    {
        public SyntaxKind Kind { get; } = Node.Kind();
    }
}
