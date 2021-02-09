#if DEBUG

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace J4JSoftware.Roslyn
{
    public class FoundSymbol
    {
        private readonly CompiledFile _compiledFile;

        public FoundSymbol( SyntaxNode node, CompiledFile compiledFile )
        {
            Node = node;
            _compiledFile = compiledFile;
        }

        public SyntaxNode Node { get; }
        public SyntaxKind NodeKind => Node.Kind();

        public ISymbol? Symbol => NodeKind switch
        {
            SyntaxKind.AttributeList => Node.Parent == null ? null : GetSymbol( Node.Parent, _compiledFile ),
            _ => GetSymbol( Node, _compiledFile )
        };

        public override string ToString() => $"{NodeKind} ({Symbol?.GetType().Name ?? "** no ISymbol found **"})";

        private ISymbol? GetSymbol( SyntaxNode node, CompiledFile compFile )
        {
            var symbolInfo = compFile.Model.GetSymbolInfo( node );

            return symbolInfo.Symbol ?? compFile.Model.GetDeclaredSymbol( node );
        }
    }
}

#endif