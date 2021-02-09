using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace J4JSoftware.Roslyn
{
    public class AttributeNodeInfo
    {
        public AttributeNodeInfo(
            SyntaxNode node,
            ISymbol? tgtSymbol,
            CompiledFile file
        )
        {
            if( node.Kind() != SyntaxKind.AttributeList )
                throw new ArgumentException(
                    $"{nameof(AttributeNodeInfo.SyntaxNode)} must be a {nameof(SyntaxKind.AttributeList)} but was a {node.Kind()} instead" );

            SyntaxNode = node;
            CompiledFile = file;
            AttributedSymbol = tgtSymbol;
        }

        public CompiledFile CompiledFile { get; }
        public SyntaxNode SyntaxNode {get;}
        public ISymbol? AttributedSymbol { get; }
    }
}