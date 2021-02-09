using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn
{
    public static class NodeCollectorExtensions
    {
        public static bool GetSymbol<TSymbol>( this SyntaxNode node, CompiledFile compFile, out TSymbol? result )
            where TSymbol : class, ISymbol
        {
            result = null;

            var symbolInfo = compFile.Model.GetSymbolInfo( node );
            var rawSymbol = symbolInfo.Symbol ?? compFile.Model.GetDeclaredSymbol( node );

            if( rawSymbol is not TSymbol retVal ) 
                return false;

            result = retVal;
            
            return true;
        }
    }
}