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
                    $"{nameof(SyntaxNode)} must be a {nameof(SyntaxKind.AttributeList)} but was a {node.Kind()} instead" );

            SyntaxNode = node;
            CompiledFile = file;
            AttributedSymbol = tgtSymbol;
        }

        public CompiledFile CompiledFile { get; }
        public SyntaxNode SyntaxNode { get; }
        public ISymbol? AttributedSymbol { get; }
    }
}