using System.Linq;
using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace J4JSoftware.Roslyn
{
    public class CompiledFile
    {
        public CompiledFile(
            SyntaxNode rootNode,
            SemanticModel model,
            CompiledProject container
        )
        {
            RootSyntaxNode = rootNode;
            Model = model;
            Container = container;
        }

        public CompiledProject Container { get; }
        public SyntaxNode RootSyntaxNode { get; }
        public SemanticModel Model { get; }

        public bool GetSymbol<TSymbol>( SyntaxNode node, out TSymbol? result )
            where TSymbol : class, ISymbol
        {
            result = null;

            var symbolInfo = Model.GetSymbolInfo( node );
            var rawSymbol = symbolInfo.Symbol ?? Model.GetDeclaredSymbol( node );

            if( rawSymbol == null )
                return false;

            if( rawSymbol is TSymbol retVal )
            {
                result = retVal;
                return true;
            }

            return false;
        }
    }

}