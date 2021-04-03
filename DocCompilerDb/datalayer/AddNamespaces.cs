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
            IFullyQualifiedNamers fqNamers,
            DocDbContext dbContext, 
            IJ4JLogger? logger ) 
            : base( fqNamers, dbContext, logger )
        {
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
            if( !Namers.GetFullyQualifiedName( nodeContext, out var fqName ) )
                return false;

            if (!Namers.GetName(nodeContext.Node, out var nsName))
                return false;

            var nsDb = DbContext.Namespaces
                .FirstOrDefault(x => x.FullyQualifiedName== fqName);

            if( nsDb == null )
            {
                nsDb = new Namespace
                {
                    FullyQualifiedName = fqName!,
                    Name = nsName!
                };

                if( !SetContainer( nodeContext, nsDb ) )
                    return false;

                DbContext.Namespaces.Add( nsDb );
            }
            else nsDb.Deprecated = false;

            DbContext.SaveChanges();

            return true;
        }

        private bool SetContainer( NodeContext nodeContext, Namespace nsDb )
        {
            if (nodeContext.Node.Parent == null)
            {
                Logger?.Error("Supplied NamespaceDeclaration node has no parent");
                return false;
            }

            var parentKind= nodeContext.Node.Parent.Kind();

            return parentKind switch
            {
                SyntaxKind.NamespaceDeclaration => SetNamespaceContainer( nodeContext, nsDb ),
                SyntaxKind.CompilationUnit => SetCodeFileContainer( nodeContext, nsDb ),
                _ => unsupported()
            };

            bool unsupported()
            {
                Logger?.Error( "Unsupported NamespaceDeclaration node parent kind '{0}'", parentKind );
                return false;
            }
        }

        private bool SetCodeFileContainer( NodeContext nodeContext, Namespace nsDb )
        {
            var cfParent = DbContext.CodeFiles
                .FirstOrDefault( x => x.FullPath == nodeContext.ScannedFile.SourceFilePath );

            if( cfParent == null )
            {
                Logger?.Error<string, string>( "Could not find containing CodeFile {0} for namespace {1}",
                    nodeContext.ScannedFile.SourceFilePath, nsDb.FullyQualifiedName );
                return false;
            }

            nsDb.SetContainer( cfParent );

            return true;
        }

        private bool SetNamespaceContainer( NodeContext nodeContext, Namespace nsDb )
        {
            if( !Namers.GetFullyQualifiedName( new NodeContext( nodeContext.Node.Parent!, nodeContext.ScannedFile ),
                out var parentFQName ) )
            {
                Logger?.Error( "Could not determine fully-qualified name of NamespaceDeclarationSyntax node's parent" );
                return false;
            }

            var nsParent = DbContext.Namespaces.FirstOrDefault( x => x.FullyQualifiedName == parentFQName! );

            if( nsParent == null )
            {
                Logger?.Error<string, string>( "Could not find containing namespace {0} for namespace {1}", parentFQName!,
                    nsDb.FullyQualifiedName );
                return false;
            }

            nsDb.SetContainer( nsParent );

            return true;
        }
    }
}