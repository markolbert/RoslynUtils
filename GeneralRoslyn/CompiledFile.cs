#region license

// Copyright 2021 Mark A. Olbert
// 
// This library or program 'GeneralRoslyn' is free software: you can redistribute it
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

            result = symbolInfo.Symbol ?? Model.GetDeclaredSymbol( node.Parent );

            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            return result != null;
        }
    }
}