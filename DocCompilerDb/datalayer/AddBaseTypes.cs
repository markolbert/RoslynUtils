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
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace J4JSoftware.DocCompiler
{
    [TopologicalPredecessor(typeof(AddDocumentedTypes))]
    public class AddBaseTypes : EntityProcessor<NodeContext>
    {
        private readonly ITypeNodeAnalyzer _nodeAnalayzer;
        private readonly ITypeReferenceResolver _typeResolver;

        public AddBaseTypes( 
            IFullyQualifiedNodeNames fqNamers,
            INodeNames namers,
            INodeIdentifierTokens nodeTokens,
            ITypeNodeAnalyzer nodeAnalzyer,
            ITypeReferenceResolver typeResolver,
            DocDbContext dbContext, 
            IJ4JLogger? logger ) 
            : base( fqNamers,namers, nodeTokens, dbContext, logger )
        {
            _nodeAnalayzer = nodeAnalzyer;
            _typeResolver = typeResolver;
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
            if( !SyntaxCollections.DocumentedTypeKinds.Any( x => nodeContext.Node.Parent.IsKind( x ) ) )
            {
                Logger?.Error("BaseList node is not a child of a Class, Interface, Record or Struct");
                return false;
            }

            if( !FullyQualifiedNames.GetName( nodeContext.Node.Parent!, out var dtNames ) )
                return false;

            if( dtNames!.Count != 1 )
            {
                Logger?.Error("Multiple alternative names for DocumentedType");
                return false;
            }

            var docTypeDb = DbContext.DocumentedTypes
                .Include(x=>x.Ancestors  )
                .FirstOrDefault( x => x.FullyQualifiedName == dtNames[0] );

            if( docTypeDb == null )
            {
                Logger?.Error<string>("Could not find DocumentedType '{0}' in the database", dtNames[0]);
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

            var ancestors = /*docTypeDb.Ancestors ?? */new List<TypeAncestor>();

            foreach( var simpleBaseNode in simpleBaseNodes )
            {
                if( !_nodeAnalayzer.Analyze( simpleBaseNode, docTypeDb, nodeContext.ScannedFile, true ) )
                    return false;

                if( !_typeResolver.Resolve( _nodeAnalayzer, docTypeDb, null, out var typeRef ) )
                    return false;

                //// no need to do anything if the ancestor is already on file
                //if( ancestors.Any( x => x.AncestorID == typeRef!.ID ) )
                //    continue;

                var newAncestor = new TypeAncestor { DeclaredByID = docTypeDb.ID };

                if( typeRef!.ID == 0 )
                    newAncestor.AncestorType = typeRef;
                else newAncestor.AncestorID = typeRef.ID;

                ancestors.Add( newAncestor );
            }

            docTypeDb.Ancestors = ancestors;

            DbContext.SaveChanges();

            return true;
        }
    }
}