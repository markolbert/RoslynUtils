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
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using Serilog;
using CSharpExtensions = Microsoft.CodeAnalysis.CSharpExtensions;

namespace J4JSoftware.DocCompiler
{
    [TopologicalPredecessor(typeof(AddAssemblies))]
    [TopologicalPredecessor(typeof(AddCodeFiles))]
    [TopologicalPredecessor(typeof(AddNamespaces))]
    [TopologicalPredecessor(typeof(AddUsings))]
    [TopologicalPredecessor(typeof(AddDocumentedTypes))]
    public class AddBaseTypes : SyntaxNodeProcessor
    {
        public static SyntaxKind[] SupportedParentKinds = new[]
        {
            SyntaxKind.ClassDeclaration,
            SyntaxKind.InterfaceDeclaration,
            SyntaxKind.RecordDeclaration,
            SyntaxKind.StructDeclaration
        };

        private readonly INamedTypeResolver _namedTypeResolver;

        public AddBaseTypes( 
            IFullyQualifiedNames fqNamers,
            INamedTypeResolver namedTypeResolver,
            DocDbContext dbContext, 
            IJ4JLogger? logger ) 
            : base( fqNamers, dbContext, logger )
        {
            _namedTypeResolver = namedTypeResolver;
        }

        protected override IEnumerable<NodeContext> GetNodesToProcess( IDocScanner source )
        {
            foreach( var scannedFile in source.ScannedFiles )
            {
                foreach( var nsNode in scannedFile.RootNode.DescendantNodes()
                    .Where( n => n.IsKind( SyntaxKind.BaseList ) ) )
                {
                    yield return new NodeContext( nsNode, scannedFile );
                }
            }
        }

        protected override bool ProcessEntity( NodeContext nodeContext )
        {
            if( !SupportedParentKinds.Any( x => nodeContext.Node.Parent.IsKind( x ) ) )
            {
                Logger?.Error("BaseList node is not a child of a Class, Interface, Record or Struct");
                return false;
            }

            if( !Namers.GetFullyQualifiedName( nodeContext.Node.Parent!, out var fqnDocType ) )
                return false;

            var docTypeDb = DbContext.DocumentedTypes
                .Include(x=>x.Ancestors  )
                .FirstOrDefault( x => x.FullyQualifiedName == fqnDocType! );

            if( docTypeDb == null )
            {
                Logger?.Error<string>("Could not find DocumentedType '{0}' in the database", fqnDocType!);
                return false;
            }

            var simpleBaseNodes = nodeContext.Node
                .ChildNodes()
                .Where( x => x.IsKind( SyntaxKind.SimpleBaseType ) )
                .ToList();

            // if we have no base types make sure there are none in the database
            // and return
            if( !simpleBaseNodes.Any() )
            {
                DbContext.TypeAncestors.RemoveRange( docTypeDb.Ancestors! );
                DbContext.SaveChanges();

                return true;
            }

            var ancestors = docTypeDb.Ancestors ?? new List<TypeAncestor>();

            foreach( var simpleBaseNode in simpleBaseNodes )
            {
                if( !_namedTypeResolver.Resolve( simpleBaseNode, docTypeDb, nodeContext.ScannedFile ) )
                    return false;

                var foundType = _namedTypeResolver.ResolvedEntity!;

                // no need to do anything if the ancestor is already on file
                if( ancestors.Any( x => x.AncestorID == foundType!.ID ) )
                    continue;

                var newAncestor = new TypeAncestor { DeclaredByID = docTypeDb.ID };

                if( foundType.ID == 0 )
                    newAncestor.AncestorType = foundType;
                else newAncestor.AncestorID = foundType.ID;

                ancestors.Add( newAncestor );
            }

            docTypeDb.Ancestors = ancestors;

            DbContext.SaveChanges();

            return true;
        }
    }
}