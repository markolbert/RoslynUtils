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

namespace J4JSoftware.DocCompiler
{
    [TopologicalPredecessor(typeof(AddTypeConstraints))]
    public class AddMethods : EntityProcessor<NodeContext>
    {
        private readonly ITypeNodeAnalyzer _tnAnalyzer;
        private readonly ITypeReferenceResolver _trResolver;

        public AddMethods( 
            IFullyQualifiedNodeNames fqNamers,
            INodeNames namers,
            INodeIdentifierTokens nodeTokens,
            ITypeNodeAnalyzer tnAnalyzer,
            ITypeReferenceResolver trResolver,
            DocDbContext dbContext, 
            IJ4JLogger? logger ) 
            : base( fqNamers, namers, nodeTokens, dbContext, logger )
        {
            _tnAnalyzer = tnAnalyzer;
            _trResolver = trResolver;
        }

        protected override IEnumerable<NodeContext> GetNodesToProcess( IDocScanner source )
        {
            foreach( var scannedFile in source.ScannedFiles )
            {
                foreach( var nsNode in scannedFile.RootNode.DescendantNodes()
                    .Where( n => n.IsKind( SyntaxKind.MethodDeclaration ) ) )
                {
                    yield return new NodeContext( nsNode, scannedFile );
                }
            }
        }

        protected override bool ProcessEntity( NodeContext nodeContext )
        {
            // find our containing node
            var containerNode = nodeContext.Node;

            while( SyntaxCollections.DocumentedTypeKinds.All( x => !containerNode.IsKind( x ) ) )
            {
                containerNode = containerNode!.Parent;

                if( containerNode == null )
                    break;
            }

            if( containerNode == null )
            {
                Logger?.Error("Could not find supported container node for MethodDeclaration");
                return false;
            }

            if( !FullyQualifiedNames.GetName( containerNode, out var containerFQNames ) )
                return false;

            if( containerFQNames!.Count != 1 )
            {
                Logger?.Error("Multiple alternative DocumentedType node names");
                return false;
            }

            var containerDb = DbContext.DocumentedTypes
                    .FirstOrDefault( x => x.FullyQualifiedName == containerFQNames[ 0 ] );

            if( containerDb == null )
            {
                Logger?.Error<string>("DocumentedType '{0}' not found in database", containerFQNames[0]  );
                return false;
            }

            if( !FullyQualifiedNames.GetName( nodeContext.Node, out var methodFQNames ) )
                return false;

            if( methodFQNames!.Count != 1 )
            {
                Logger?.Error("Multiple alternative fully-qualified method names");
                return false;
            }

            if( !Names.GetName( nodeContext.Node, out var methodName ) )
                return false;

            var methodDb = DbContext.Methods
                .Include( x => x.Arguments )
                .FirstOrDefault( x => x.FullyQualifiedName == methodFQNames[0] );

            if( methodDb == null )
            {
                methodDb = new Method
                {
                    Name = methodName!,
                    DeclaredIn = new List<DocumentedType> { containerDb }
                };

                DbContext.Methods.Add( methodDb );
            }
            else methodDb.Deprecated = false;

            methodDb.FullyQualifiedName = methodFQNames[0];

            methodDb.Accessibility = nodeContext.Node.GetAccessibility();
            methodDb.IsAbstract = nodeContext.Node.HasChildNode( SyntaxKind.AbstractKeyword );
            methodDb.IsStatic = nodeContext.Node.HasChildNode(SyntaxKind.StaticKeyword );
            methodDb.IsNew = nodeContext.Node.HasChildNode(SyntaxKind.NewKeyword );
            methodDb.IsVirtual = nodeContext.Node.HasChildNode(SyntaxKind.VirtualKeyword );
            methodDb.IsHidden = nodeContext.Node.HasChildNode(SyntaxKind.HiddenKeyword );
            methodDb.IsOverride = nodeContext.Node.HasChildNode(SyntaxKind.OverrideKeyword );

            if( !SetReturnType( nodeContext.Node, containerDb, nodeContext.ScannedFile, methodDb ) )
                return false;

            if( !ProcessArguments( nodeContext.Node, methodDb ) )
                return false;

            DbContext.SaveChanges();

            return true;
        }

        private bool SetReturnType( SyntaxNode node, DocumentedType dtDb, IScannedFile scannedFile, Method methodDb )
        {
            var rtNode = node.ChildNodes()
                .FirstOrDefault( x => SyntaxCollections.TypeNodeKinds.Any( x.IsKind ) );

            if( rtNode == null )
            {
                Logger?.Error<string>( "Could not find return type SyntaxNode for method '{0}'",
                    methodDb.FullyQualifiedName );
                return false;
            }

            if( !_tnAnalyzer.Analyze( rtNode, dtDb, scannedFile ) )
            {
                Logger?.Error<string>( "Could not analyze return type SyntaxNode for method '{0}'",
                    methodDb.FullyQualifiedName );
                return false;
            }

            if( !_trResolver.Resolve( _tnAnalyzer, dtDb, null, out var typeRef ) )
                return false;

            if( typeRef!.ID == 0 )
                methodDb.ReturnType = typeRef;
            else methodDb.ReturnTypeID = typeRef.ID;

            return true;
        }

        private bool ProcessArguments( SyntaxNode node, Method methodDb )
        {
            return true;
        }
    }
}