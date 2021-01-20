using System;
using System.Collections.Generic;
using J4JSoftware.Utilities;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn
{
    public enum NodeSinkResult
    {
        Okay,
        AlreadyProcessed,
        IgnorableNode,
        InvalidNode,
        TerminalNode,
        UnsupportedNode
    }

    public interface ISingleWalker : IAction<CompiledProject>
    {

    }

    public interface ISyntaxNodeSink
    {
        bool AlreadyProcessed( SyntaxNode node );
        bool ProcessesNode(SyntaxNode node);
        bool DrillIntoNode( SyntaxNode node );

        bool InitializeSink(SemanticModel model);
        bool FinalizeSink(ISingleWalker syntaxWalker);
        void OutputSyntaxNode( Stack<SyntaxNode> nodeStack );
    }

    public interface ISymbolSink
    {
        bool SupportsSymbol( Type symbolType );
        bool InitializeSink( ISyntaxWalker syntaxWalker );
        bool FinalizeSink( ISyntaxWalker syntaxWalker );
        bool OutputSymbol(ISyntaxWalker syntaxWalker, ISymbol symbol );
    }

    public interface ISymbolSink<in TSymbol> : ISymbolSink
        where TSymbol : ISymbol
    {
        bool OutputSymbol( ISyntaxWalker syntaxWalker, TSymbol symbol );
    }

    public interface IDefaultSymbolSink : ISymbolSink
    {
    }
}