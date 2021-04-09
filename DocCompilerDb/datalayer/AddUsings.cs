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
    [TopologicalPredecessor(typeof(AddNamespaces))]
    public class AddUsings : SyntaxNodeProcessor
    {
        public AddUsings( 
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
                    .Where( n => n.IsKind( SyntaxKind.UsingDirective ) ) )
                {
                    yield return new NodeContext( nsNode, scannedFile );
                }
            }
        }

        protected override bool ProcessEntity( NodeContext nodeContext )
        {
            if( !Namers.GetFullyQualifiedName( nodeContext.Node, out var fqUsing ) )
                return false;

            if( !Namers.GetName( nodeContext.Node, out var usingName ) )
                return false;

            var assemblyDb = DbContext.Assemblies
                .FirstOrDefault( x => x.AssemblyName == nodeContext.ScannedFile.BelongsTo.AssemblyName );

            if( assemblyDb == null )
            {
                Logger?.Error<string>( "Could not find Assembly '{0}' in database",
                    nodeContext.ScannedFile.BelongsTo.AssemblyName );

                return false;
            }

            var extNsDb = DbContext.Namespaces
                .Include( x => x.CodeFiles )
                .Include( x => x.Assemblies )
                .FirstOrDefault( x => x.FullyQualifiedName == fqUsing );

            // if we find the namespace already listed it's defined within the scope of the
            // documentation project and we're done
            if( extNsDb != null )
                return true;

            extNsDb = new Namespace
            {
                Name = usingName!,
                FullyQualifiedName = fqUsing!,
                InDocumentationScope = false,
                Assemblies = new List<Assembly> { assemblyDb }
            };

            if( !ProcessAlias( nodeContext.Node, extNsDb, assemblyDb ) )
                return false;

            if( !SetContainer( nodeContext, extNsDb ) )
                return false;

            DbContext.Namespaces.Add( extNsDb );

            DbContext.SaveChanges();

            return true;
        }

        private bool ProcessAlias( SyntaxNode node, Namespace extNsDb, Assembly assemblyDb )
        {
            // a NameEquals node declares this Using to be an alias
            var nameEqualsNode = node.ChildNodes().FirstOrDefault( x => x.IsKind( SyntaxKind.NameEquals ) );

            if( nameEqualsNode == null )
                return true;

            // the target of the alias is contained in the next node, which must be either an 
            // IdentifierName or a QualifiedName
            var siblingNodes = node.ChildNodes().ToList();

            var nameEqualsIndex = siblingNodes.FindIndex( x => ReferenceEquals( x, nameEqualsNode ) );

            if( nameEqualsIndex < 0 )
            {
                Logger?.Error<string>(
                    "Could not find NameEquals node in sibling node collection for Using alias '{0}'", extNsDb.Name );
                return false;
            }

            var aliasSrcName = siblingNodes[ nameEqualsIndex + 1 ].ToString();

            var aliasSrc = DbContext.Namespaces.FirstOrDefault( x => x.FullyQualifiedName == aliasSrcName );

            if( aliasSrc == null )
            {
                aliasSrc = new Namespace
                {
                    Name = aliasSrcName,
                    FullyQualifiedName = aliasSrcName,
                    InDocumentationScope = false,
                    Assemblies = new List<Assembly> { assemblyDb }
                };

                DbContext.Namespaces.Add( aliasSrc );

                extNsDb.AliasedNamespace = aliasSrc;
            }
            else extNsDb.AliasedNamespaceID = aliasSrc.ID;

            return true;
        }

        private bool SetContainer( NodeContext nodeContext, Namespace extNsDb )
        {
            if( nodeContext.Node.Parent == null )
            {
                Logger?.Error("Using statement '{0}' does not have a parent node");
                return false;
            }

            var parentKind = nodeContext.Node.Parent.Kind();

            var cfDb = DbContext.CodeFiles
                .FirstOrDefault( x => x.FullPath == nodeContext.ScannedFile.SourceFilePath );

            if( cfDb != null )
                return parentKind switch
                {
                    SyntaxKind.CompilationUnit => SetCodeFileContainer( cfDb, extNsDb ),
                    SyntaxKind.NamespaceDeclaration => SetNamespaceContainer( nodeContext.Node.Parent, extNsDb ),
                    _ => unsupported()
                };

            Logger?.Error<string, string>(
                "Could not find containing code file '{0}' for using node '{1}' in database",
                nodeContext.ScannedFile.SourceFilePath, 
                extNsDb.Name );

            return false;

            bool unsupported()
            {
                Logger?.Error("Unsupported Using container type '{0}'", parentKind  );
                return false;
            }
        }

        private bool SetCodeFileContainer( CodeFile cfDb, Namespace extNsDb )
        {
            if( extNsDb.CodeFiles == null )
                extNsDb.CodeFiles = new List<CodeFile> { cfDb };
            else
            {
                if( extNsDb.CodeFiles.All( x => x.ID != cfDb.ID ) )
                    extNsDb.CodeFiles.Add( cfDb );
            }

            return true;
        }

        private bool SetNamespaceContainer( SyntaxNode containingNode, Namespace extNsDb )
        {
            if( !Namers.GetFullyQualifiedName( containingNode, out var fqContaining ) )
            {
                Logger?.Error<string>( "Couldn't find containing Namespace for Using node '{0}'",
                    extNsDb.FullyQualifiedName );
                
                return false;
            }

            var containingNsDb = DbContext.Namespaces
                .Include( x => x.ChildNamespaces )
                .FirstOrDefault( x => x.FullyQualifiedName == fqContaining );

            if( containingNsDb == null )
            {
                Logger?.Error<string, string>(
                    "Couldn't find containing Namespace entity '{0}' in database for Using node '{1}'",
                    fqContaining!,
                    extNsDb.FullyQualifiedName );
                
                return false;
            }

            if( containingNsDb.ChildNamespaces == null )
                containingNsDb.ChildNamespaces = new List<Namespace> { extNsDb };
            else
            {
                if( containingNsDb.ChildNamespaces!.All( x => x.ID != extNsDb.ID ) )
                    containingNsDb.ChildNamespaces!.Add( extNsDb );
            }

            return true;
        }
    }
}