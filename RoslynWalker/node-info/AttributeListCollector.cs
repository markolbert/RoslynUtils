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