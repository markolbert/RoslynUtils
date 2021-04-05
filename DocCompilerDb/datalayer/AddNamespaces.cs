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
using Microsoft.EntityFrameworkCore;
using Serilog;
using CSharpExtensions = Microsoft.CodeAnalysis.CSharpExtensions;

namespace J4JSoftware.DocCompiler
{
    [TopologicalPredecessor(typeof(AddAssemblies))]
    [TopologicalPredecessor(typeof(AddCodeFiles))]
    public class AddNamespaces : SyntaxNodeProcessor
    {
        public AddNamespaces( 
            IFullyQualifiedNames fqNamers,
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
            if( !Namers.GetFullyQualifiedName( nodeContext.Node, out var fqName ) )
                return false;

            if (!Namers.GetName(nodeContext.Node, out var nsName))
                return false;

            var assemblyDb = DbContext.Assemblies
                .FirstOrDefault( x => x.AssemblyName == nodeContext.ScannedFile.BelongsTo.AssemblyName );

            if( assemblyDb == null )
            {
                Logger?.Error<string>( "Could not find Assembly '{0}' in database",
                    nodeContext.ScannedFile.BelongsTo.AssemblyName );

                return false;
            }

            if (!GetParentNamespace(nodeContext, out var nsParentDb))
                return false;

            var nsDb = DbContext.Namespaces
                .Include(x=>x.Assemblies)
                .FirstOrDefault(x => x.FullyQualifiedName== fqName);

            if( nsDb == null )
            {
                nsDb = new Namespace
                {
                    FullyQualifiedName = fqName!,
                    Name = nsName!,
                    Assemblies = new List<Assembly> { assemblyDb },
                    ContainingNamespaceID = nsParentDb?.ID
                };

                DbContext.Namespaces.Add( nsDb );
            }
            else
            {
                nsDb.Deprecated = false;

                if( nsDb.Assemblies.All( x => x.ID != assemblyDb.ID ) )
                    nsDb.Assemblies.Add( assemblyDb );

                nsDb.ContainingNamespace = nsParentDb;
            }

            DbContext.SaveChanges();

            return true;
        }

        private bool GetParentNamespace( NodeContext nodeContext, out Namespace? result )
        {
            result = null;

            if( nodeContext.Node.Parent == null )
                return true;

            var parentKind= nodeContext.Node.Parent.Kind();

            if (parentKind != SyntaxKind.NamespaceDeclaration)
                return true;

            if (!Namers.GetFullyQualifiedName(nodeContext.Node.Parent!, out var parentFQName))
            {
                Logger?.Error("Could not determine fully-qualified name of NamespaceDeclarationSyntax node's parent");
                return false;
            }

            result = DbContext.Namespaces.FirstOrDefault(x => x.FullyQualifiedName == parentFQName!);

            if (result == null)
            {
                Logger?.Error<string>("Could not find containing namespace {0}", parentFQName!);
                return false;
            }

            return true;
        }
    }
}