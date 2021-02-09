using System.Collections;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace J4JSoftware.Roslyn
{
    public class AttributeListCollector : INodeCollectorBase
    {
        internal AttributeListCollector( NodeCollectors container )
        {
            Container = container;
        }

        public NodeCollectors Container { get; }

        public StoreSymbolResult StoreSymbol( SyntaxNode node, CompiledFile compiledFile, out ISymbol? result )
        {
            result = null;

            // get the symbol targeted by this attribute
            if( node.Kind() != SyntaxKind.AttributeList || node.Parent == null )
                return StoreSymbolResult.NotFound;

            result = GetSymbol( node.Parent, compiledFile );

            Container.StoreAttributeList( node, result, compiledFile );

            // null target is expected for attributes attached to compilation units
            return result != null || node.Parent.Kind() == SyntaxKind.CompilationUnit
                ? StoreSymbolResult.Stored
                : StoreSymbolResult.NotFound;
        }

        private ISymbol? GetSymbol( SyntaxNode node, CompiledFile compFile )
        {
            var symbolInfo = compFile.Model.GetSymbolInfo( node );

            return symbolInfo.Symbol ?? compFile.Model.GetDeclaredSymbol( node );
        }
    }
}