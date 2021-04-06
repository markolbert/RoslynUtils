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
            if (!Namers.GetName(nodeContext.Node, out var usingName))
                return false;

            var assemblyDb = DbContext.Assemblies
                .FirstOrDefault( x => x.AssemblyName == nodeContext.ScannedFile.BelongsTo.AssemblyName );

            if( assemblyDb == null )
            {
                Logger?.Error<string>( "Could not find Assembly '{0}' in database",
                    nodeContext.ScannedFile.BelongsTo.AssemblyName );

                return false;
            }

            var usingDB = DbContext.Usings
                .Include(x=>x.CodeFiles)
                .Include(x=>x.Namespaces  )
                .FirstOrDefault(x => x.Name == usingName);

            if( usingDB == null )
            {
                usingDB = new Using
                {
                    Name = usingName!,
                    AssemblyID = assemblyDb.ID
                };

                DbContext.Usings.Add( usingDB );
            }
            else usingDB.Deprecated = false;

            if( !SetContainer( nodeContext, usingDB ) )
                return false;

            DbContext.SaveChanges();

            return true;
        }

        private bool SetContainer( NodeContext nodeContext, Using usingDb )
        {
            if( nodeContext.Node.Parent == null )
            {
                Logger?.Error("Using statement '{0}' does not have a parent node");
                return false;
            }

            var parentKind = nodeContext.Node.Parent.Kind();

            return parentKind switch
            {
                SyntaxKind.CompilationUnit => SetCodeFileContainer(nodeContext.ScannedFile, usingDb),
                SyntaxKind.NamespaceDeclaration=>SetNamespaceContainer(nodeContext.Node.Parent, usingDb),
                _ => unsupported()
            };

            bool unsupported()
            {
                Logger?.Error("Unsupported Using container type '{0}'", parentKind  );
                return false;
            }
        }

        private bool SetCodeFileContainer( IScannedFile scannedFile, Using usingDb )
        {
            var cfDb = DbContext.CodeFiles
                .FirstOrDefault( x => x.FullPath == scannedFile.SourceFilePath );

            if( cfDb == null )
            {
                Logger?.Error<string, string>(
                    "Could not find containing code file '{0}' for using node '{1}' in database",
                    scannedFile.SourceFilePath, 
                    usingDb.Name );

                return false;
            }

            if( usingDb.CodeFiles == null )
                usingDb.CodeFiles = new List<CodeFile> { cfDb };
            else
            {
                if( usingDb.CodeFiles.All( x => x.ID != cfDb.ID ) )
                    usingDb.CodeFiles.Add( cfDb );
            }

            return true;
        }

        private bool SetNamespaceContainer( SyntaxNode containerNode, Using usingDb )
        {
            if( !Namers.GetFullyQualifiedName( containerNode, out var nsFQN ) )
                return false;

            var nsDb = DbContext.Namespaces
                .FirstOrDefault( x => x.FullyQualifiedName == nsFQN );

            if( nsDb == null )
            {
                Logger?.Error<string, string>(
                    "Could not find containing namespace '{0}' for using node '{1}' in database",
                    nsFQN!, 
                    usingDb.Name );

                return false;
            }

            if( usingDb.Namespaces == null )
                usingDb.Namespaces = new List<Namespace> { nsDb };
            else
            {
                if( usingDb.Namespaces.All( x => x.ID != nsDb.ID ) )
                    usingDb.Namespaces.Add( nsDb );
            }

            return true;
        }
    }
}