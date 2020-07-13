using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn
{
    public class CompilationResult
    {
        public CompilationResult( SyntaxNode syntax, SemanticModel model )
        {
            Syntax = syntax;
            Model = model;
        }

        public SyntaxNode Syntax { get; }
        public SemanticModel Model { get; }
    }
}