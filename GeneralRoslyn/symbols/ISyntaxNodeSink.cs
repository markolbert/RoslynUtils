using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn
{
    public interface ISyntaxNodeSink
    {
        bool AlreadyProcessed( SyntaxNode node );
        bool ProcessesNode(SyntaxNode node);
        bool DrillIntoNode( SyntaxNode node );

        bool InitializeSink(SemanticModel model);
        bool FinalizeSink(ISyntaxWalker syntaxWalker);
        void OutputSyntaxNode( Stack<SyntaxNode> nodeStack );
    }
}