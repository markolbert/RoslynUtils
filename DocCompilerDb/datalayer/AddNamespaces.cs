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
using System.Text;
using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Serilog;
using CSharpExtensions = Microsoft.CodeAnalysis.CSharpExtensions;

namespace J4JSoftware.DocCompiler
{
    [TopologicalPredecessor(typeof(AddAssemblies))]
    [TopologicalPredecessor(typeof(AddCodeFiles))]
    public class AddNamespaces : EntityProcessor<NodeContext>
    {
        public AddNamespaces( 
            DocDbContext dbContext, 
            IJ4JLogger? logger ) 
            : base( dbContext, logger )
        {
        }

        public override bool GetFullyQualifiedName( NodeContext nodeContext, out string? result )
        {
            result = null;

            if( nodeContext.Kind != SyntaxKind.NamespaceDeclaration )
            {
                Logger?.Error("Supplied SyntaxNode is not a NamespaceDeclaration"  );
                return false;
            }

            if( !GetName( nodeContext.Node, out var startName ) )
                return false;

            var sb = new StringBuilder(startName!);

            var curNode = nodeContext.Node;

            while( ( curNode = curNode.Parent ) != null && curNode.Kind() == SyntaxKind.NamespaceDeclaration )
            {
                if (!GetName(curNode, out var curName))
                    return false;

                sb.Insert( 0, $"{curName}." );
            }

            result = sb.ToString();

            return !string.IsNullOrEmpty( result );
        }

        protected override IEnumerable<NodeContext> GetNodesToProcess( IDocScanner source )
        {
            foreach( var scannedFile in source.ScannedFiles )
            {
                foreach( var nsNode in scannedFile.RootNode.DescendantNodes()
                    .Where( n => n.IsKind( SyntaxKind.NamespaceDeclaration ) ) )
                {
                    yield return new NodeContext( nsNode, scannedFile );
                }
            }
        }

        protected override bool ProcessEntity( NodeContext nodeContext )
        {
            if( !GetContainer( nodeContext, out var parentKind, out var parent ) )
                return false;

            if( !GetFullyQualifiedName( nodeContext, out var fqName ) )
                return false;

            if (!GetName(nodeContext.Node, out var nsName))
                return false;

            var nsDb = DbContext.Namespaces
                .FirstOrDefault(x => x.FullyQualifiedName== fqName);

            // we don't try to update the namespace's container, as that may not be defined yet
            // if it's a class
            if( nsDb == null )
            {
                nsDb = new Namespace
                {
                    FullyQualifiedName = fqName!,
                    Name = nsName!
                };

                if( parentKind == SyntaxKind.NamespaceDeclaration )
                    nsDb.SetContainer( (Namespace) parent! );
                else nsDb.SetContainer( (CodeFile) parent! );

                DbContext.Namespaces.Add( nsDb );
            }
            else nsDb.Deprecated = false;

            DbContext.SaveChanges();

            return true;
        }

        private bool GetContainer( NodeContext nodeContext, out SyntaxKind? parentKind, out object? parent )
        {
            parentKind = null;
            parent = null;
            
            if (nodeContext.Kind != SyntaxKind.NamespaceDeclaration)
            {
                Logger?.Error("Supplied SyntaxNode is not a NamespaceDeclaration");
                return false;
            }

            if (nodeContext.Node.Parent == null)
            {
                Logger?.Error("Supplied NamespaceDeclaration node has no parent");
                return false;
            }

            parentKind= nodeContext.Node.Parent.Kind();

            switch( parentKind )
            {
                case SyntaxKind.NamespaceDeclaration:
                    if( !GetFullyQualifiedName( new NodeContext( nodeContext.Node.Parent!, nodeContext.ScannedFile ),
                        out var parentFQName ) )
                    {
                        Logger?.Error("Could not determine fully-qualified name of NamespaceDeclarationSyntax node's parent");
                        return false;
                    }

                    parent = DbContext.Namespaces.FirstOrDefault( x => x.FullyQualifiedName == parentFQName! );

                    break;

                case SyntaxKind.CompilationUnit:
                    parent = DbContext.CodeFiles
                        .FirstOrDefault( x => x.FullPath == nodeContext.ScannedFile.SourceFilePath );
                    break;

                default:
                    Logger?.Error("Unsupported NamespaceDeclaration node parent kind '{0}'", parentKind);
                    return false;
            }

            return parent != null;
        }
    }
}