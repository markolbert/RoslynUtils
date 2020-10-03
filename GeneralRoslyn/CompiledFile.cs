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

            //if( rawSymbol == null )
            //    return false;

            if( rawSymbol is TSymbol retVal )
            {
                result = retVal;
                return true;
            }

            return false;
        }

        public bool GetAttributableSymbol( SyntaxNode node, out ISymbol? result )
        {
            result = null;

            if( node.Kind() != SyntaxKind.AttributeList )
                return false;

            if( node.Parent == null )
                return false;

            var symbolInfo = Model.GetSymbolInfo( node.Parent );

            result  = symbolInfo.Symbol ?? Model.GetDeclaredSymbol(node.Parent);

            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            return result != null;
        }
    }

}