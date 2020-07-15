using System.Linq;
using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn
{
    public class CompilationResult
    {
        public CompilationResult(
            SyntaxNode rootNode,
            SemanticModel model,
            CompilationResults context
        )
        {
            RootSyntaxNode = rootNode;
            Model = model;
            Context = context;
        }

        public CompilationResults Context { get; }
        public SyntaxNode RootSyntaxNode { get; }
        public SemanticModel Model { get; }

        public bool GetSymbol<TSymbol>( SyntaxNode node, out TSymbol? result )
            where TSymbol : class, ISymbol
        {
            result = null;

            var symbolInfo = Model.GetSymbolInfo( node );

            if( symbolInfo.Symbol == null )
                return false;

            if( symbolInfo.Symbol is TSymbol retVal )
            {
                result = retVal;
                return true;
            }

            return false;
        }
    }

}