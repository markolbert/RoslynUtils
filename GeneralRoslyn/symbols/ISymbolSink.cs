using System;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn
{
    public enum NodeSinkResult
    {
        Okay,
        AlreadyProcessed,
        UnsupportedSyntaxNode
    }

    public interface ISingleWalker : ITopologicalAction<CompiledProject>
    {

    }

    public interface ISyntaxNodeSink
    {
        bool InitializeSink(SemanticModel model);
        bool FinalizeSink(ISingleWalker syntaxWalker);
        NodeSinkResult OutputSyntaxNode(SyntaxNode node);
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