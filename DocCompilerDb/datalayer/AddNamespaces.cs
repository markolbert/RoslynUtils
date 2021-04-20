﻿#region license

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
using Microsoft.EntityFrameworkCore;

namespace J4JSoftware.DocCompiler
{
    [TopologicalPredecessor(typeof(AddCodeFiles))]
    public class AddNamespaces : EntityProcessor<NodeContext>
    {
        public AddNamespaces( 
            IFullyQualifiedNodeNames fqNamers,
            INodeNames namers,
            INodeIdentifierTokens nodeTokens,
            DocDbContext dbContext, 
            IJ4JLogger? logger ) 
            : base( fqNamers, namers, nodeTokens, dbContext, logger )
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
            if( !FullyQualifiedNames.GetName( nodeContext.Node, out var fqNames ) )
                return false;

            if( fqNames!.Count != 1 )
            {
                Logger?.Error("Multiple alternative Namespace names");
                return false;
            }

            if (!Names.GetName(nodeContext.Node, out var nsName))
                return false;

            var assemblyDb = DbContext.Assemblies
                .FirstOrDefault( x => x.AssemblyName == nodeContext.ScannedFile.BelongsTo.AssemblyName );

            if( assemblyDb == null )
            {
                Logger?.Error<string>( "Could not find Assembly '{0}' in database",
                    nodeContext.ScannedFile.BelongsTo.AssemblyName );

                return false;
            }

            var codeFileDb = DbContext.CodeFiles
                .FirstOrDefault( x => x.FullPath == nodeContext.ScannedFile.SourceFilePath );

            if( codeFileDb == null )
            {
                Logger?.Error<string>("Could not find CodeFile '{0}' in database",
                    nodeContext.ScannedFile.SourceFilePath);

                return false;
            }

            if (!GetParentNamespace(nodeContext, out var nsParentDb))
                return false;

            var nsDb = DbContext.Namespaces
                .Include(x=>x.Assemblies)
                .Include(x=>x.CodeFiles)
                .FirstOrDefault(x => x.FullyQualifiedName== fqNames[0]);

            if( nsDb == null )
            {
                nsDb = new Namespace
                {
                    FullyQualifiedName = fqNames[0],
                    Name = nsName!,
                    Assemblies = new List<Assembly> { assemblyDb },
                    InDocumentationScope = true,
                    CodeFiles = new List<CodeFile> { codeFileDb},
                    ContainingNamespaceID = nsParentDb?.ID
                };

                DbContext.Namespaces.Add( nsDb );
            }
            else
            {
                nsDb.Deprecated = false;
                nsDb.InDocumentationScope = true;

                if( nsDb.Assemblies == null )
                    nsDb.Assemblies = new List<Assembly> { assemblyDb };
                else
                {
                    if( nsDb.Assemblies.All( x => x.ID != assemblyDb.ID ) )
                        nsDb.Assemblies.Add( assemblyDb );
                }

                if( nsDb.CodeFiles == null )
                    nsDb.CodeFiles = new List<CodeFile> { codeFileDb };
                else
                {
                    if( nsDb.CodeFiles.All( x => x.ID != codeFileDb.ID ) )
                        nsDb.CodeFiles.Add( codeFileDb );
                }

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

            if (!FullyQualifiedNames.GetName(nodeContext.Node.Parent!, out var parentFQNames))
            {
                Logger?.Error("Could not determine fully-qualified name of NamespaceDeclarationSyntax node's parent");
                return false;
            }

            if( parentFQNames!.Count != 1 )
            {
                Logger?.Error("Multiple alternative fully-qualified parent Namespace names");
                return false;
            }

            result = DbContext.Namespaces.FirstOrDefault(x => x.FullyQualifiedName == parentFQNames[0]);

            if( result != null ) 
                return true;

            Logger?.Error<string>("Could not find containing namespace {0}", parentFQNames[0]);
            return false;
        }
    }
}