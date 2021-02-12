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