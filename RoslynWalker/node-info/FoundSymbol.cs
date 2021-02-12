#region license

// Copyright 2021 Mark A. Olbert
// 
// This library or program 'RoslynWalker' is free software: you can redistribute it
// and/or modify it under the terms of the GNU General Public License as
// published by the Free Software Foundation, either version 3 of the License,
// or (at your option) any later version.
// 
// This library or program is distributed in the hope that it will be useful, but
// WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// General Public License for more details.
// 
// You should have received a copy of the GNU General Public License along with
// this library or program.  If not, see <https://www.gnu.org/licenses/>.

#endregion

#if DEBUG

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

        public override string ToString()
        {
            return $"{NodeKind} ({Symbol?.GetType().Name ?? "** no ISymbol found **"})";
        }

        private ISymbol? GetSymbol( SyntaxNode node, CompiledFile compFile )
        {
            var symbolInfo = compFile.Model.GetSymbolInfo( node );

            return symbolInfo.Symbol ?? compFile.Model.GetDeclaredSymbol( node );
        }
    }
}

#endif