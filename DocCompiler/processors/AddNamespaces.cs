#region license

// Copyright 2021 Mark A. Olbert
// 
// This library or program 'DocCompiler' is free software: you can redistribute it
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

using System.Collections.Generic;
using System.Linq;
using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using CSharpExtensions = Microsoft.CodeAnalysis.CSharpExtensions;

namespace J4JSoftware.DocCompiler
{
    [TopologicalPredecessor(typeof(AddAssemblies))]
    [TopologicalPredecessor(typeof(AddCodeFiles))]
    public class AddNamespaces : EntityProcessor<SyntaxNode>
    {
        public AddNamespaces( 
            IDataLayer dataLayer, 
            IJ4JLogger? logger ) 
            : base( dataLayer, logger )
        {
        }

        protected override IEnumerable<SyntaxNode> GetNodesToProcess( IDocScanner source ) =>
            source.ScannedFiles.SelectMany( x => x.RootNode
                .DescendantNodes()
                .Where( n => ((SyntaxNode?) n).IsKind( SyntaxKind.NamespaceDeclaration ) )
            );

        protected override bool ProcessEntity( SyntaxNode srcEntity ) =>
            DataLayer.UpdateNamespace( srcEntity );
    }
}