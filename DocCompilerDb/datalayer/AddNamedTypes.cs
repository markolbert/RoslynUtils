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
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Serilog;
using CSharpExtensions = Microsoft.CodeAnalysis.CSharpExtensions;

namespace J4JSoftware.DocCompiler
{
    [TopologicalPredecessor(typeof(AddAssemblies))]
    [TopologicalPredecessor(typeof(AddCodeFiles))]
    public class AddNamedTypes : EntityProcessor<NodeContext>
    {
        public static SyntaxKind[] SupportedKinds = new[]
        {
            SyntaxKind.ClassDeclaration,
            SyntaxKind.InterfaceDeclaration,
            SyntaxKind.RecordDeclaration,
            SyntaxKind.StructDeclaration
        };

        public AddNamedTypes( 
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
                    .Where( n => SupportedKinds.Any(x=>x == n.Kind()) ) )
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

            var ntDb = DbContext.NamedTypes
                .FirstOrDefault(x => x.FullyQualifiedName== fqName);

            if( ntDb == null )
            {
                ntDb = new NamedType
                {
                    FullyQualifiedName = fqName!,
                    Name = nsName!
                };

                if( !SetContainer( nodeContext, ntDb ) )
                    return false;

                DbContext.NamedTypes.Add( ntDb );
            }
            else ntDb.Deprecated = false;

            DbContext.SaveChanges();

            return true;
        }

        private bool SetContainer(NodeContext nodeContext, NamedType ntDb )
        {
            if (nodeContext.Node.Parent == null)
            {
                Logger?.Error("Supplied named type declaration node has no parent");
                return false;
            }

            var parentKind = nodeContext.Node.Parent.Kind();

            return parentKind switch
            {
                SyntaxKind.NamespaceDeclaration => SetNamespaceContainer(nodeContext, ntDb),
                SyntaxKind.CompilationUnit => SetCodeFileContainer(nodeContext, ntDb),
                SyntaxKind.ClassDeclaration=>SetNamedTypeContainer(nodeContext, ntDb),
                SyntaxKind.InterfaceDeclaration => SetNamedTypeContainer(nodeContext, ntDb),
                SyntaxKind.RecordDeclaration => SetNamedTypeContainer(nodeContext, ntDb),
                SyntaxKind.StructDeclaration => SetNamedTypeContainer(nodeContext, ntDb),
                _ => unsupported( parentKind )
            };

            bool unsupported( SyntaxKind theKind )
            {
                Logger?.Error("Unsupported NamespaceDeclaration node parent kind '{0}'", nodeContext.Node.Parent.Kind());
                return false;
            }
        }

        private bool SetNamedTypeContainer( NodeContext nodeContext, NamedType ntDb )
        {
            if( !Namers.GetFullyQualifiedName(
                new NodeContext( nodeContext.Node.Parent!, nodeContext.ScannedFile ),
                out var parentFQName ) )
            {
                Logger?.Error<string>(
                    "Could not determine fully-qualified name of parent named type node for {0}",
                    ntDb.FullyQualifiedName );

                return false;
            }

            var ntParent = DbContext.NamedTypes
                .FirstOrDefault( x => x.FullyQualifiedName == parentFQName! );

            if( ntParent == null )
            {
                Logger?.Error<string>(
                    "Could not find parent named type node {0}", parentFQName! );

                return false;
            }

            ntDb.SetContainer( ntParent );

            return true;
        }

        private bool SetCodeFileContainer( NodeContext nodeContext, NamedType ntDb )
        {
            var cfParent = DbContext.CodeFiles
                .FirstOrDefault( x => x.FullPath == nodeContext.ScannedFile.SourceFilePath );

            if( cfParent == null )
            {
                Logger?.Error<string>(
                    "Could not find parent NamespaceDeclarationSyntax node {0}",
                    nodeContext.ScannedFile.SourceFilePath );

                return false;
            }

            ntDb.SetContainer( cfParent );

            return true;
        }

        private bool SetNamespaceContainer( NodeContext nodeContext, NamedType ntDb )
        {
            if( !Namers.GetFullyQualifiedName( new NodeContext( nodeContext.Node.Parent!, nodeContext.ScannedFile ),
                out var nsFQName ) )
            {
                Logger?.Error<string>(
                    "Could not determine fully-qualified name of parent NamespaceDeclarationSyntax node for {0}",
                    ntDb.FullyQualifiedName );

                return false;
            }

            var nsParent = DbContext.Namespaces.FirstOrDefault( x => x.FullyQualifiedName == nsFQName! );

            if( nsParent == null )
            {
                Logger?.Error<string>(
                    "Could not find parent NamespaceDeclarationSyntax node {0}",
                    nsFQName! );

                return false;
            }

            ntDb.SetContainer( nsParent );
            return true;
        }
    }
}