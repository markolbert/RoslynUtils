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
    [TopologicalPredecessor(typeof(AddUsings))]
    public class AddDocumentedTypes : EntityProcessor<NodeContext>
    {
        public AddDocumentedTypes( 
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
                    .Where( n => SyntaxCollections.DocumentedTypeKinds.Any(x=>x == n.Kind()) ) )
                {
                    yield return new NodeContext( nsNode, scannedFile );
                }
            }
        }

        protected override bool ProcessEntity( NodeContext nodeContext )
        {
            if( !FullyQualifiedNames.GetName( nodeContext.Node, out var dtFQNames ) )
                return false;

            if( dtFQNames!.Count != 1 )
            {
                Logger?.Error("Multiple alternative names for DocumentedType");
                return false;
            }

            if (!Names.GetName(nodeContext.Node, out var dtSimpleName))
                return false;

            if( !FullyQualifiedNames.GetName( nodeContext.Node, out var nonGenericNames, false ) )
                return false;

            if( nonGenericNames!.Count != 1 )
            {
                Logger?.Error("Multiple alternative non-generic names for DocumentedType");
                return false;
            }

            var codeFileDb = DbContext.CodeFiles
                .FirstOrDefault( x => x.FullPath == nodeContext.ScannedFile.SourceFilePath );

            if( codeFileDb == null )
            {
                Logger?.Error<string>( "Could not find CodeFile reference for '{0}'",
                    nodeContext.ScannedFile.SourceFilePath );

                return false;
            }

            var typeConstraints = nodeContext.Node.ChildNodes()
                    .Where( x => x.IsKind( SyntaxKind.TypeParameterList ) )
                    .ToList()
                    .SelectMany(x=>x.GetTypeParameterInfo())
                    .ToList();

            var dtDb = DbContext.DocumentedTypes
                .Include( x => x.CodeFiles )
                .Include( x => x.TypeParameters )
                .FirstOrDefault( x => x.FullyQualifiedNameWithoutTypeParameters == nonGenericNames[0]
                                      && x.NumTypeParameters == typeConstraints.Count );

            if( dtDb == null )
            {
                dtDb = new DocumentedType
                {
                    FullyQualifiedNameWithoutTypeParameters = nonGenericNames[0],
                    Name = dtSimpleName!,
                    NumTypeParameters = typeConstraints.Count,
                    CodeFiles = new List<CodeFile> { codeFileDb}
                };

                DbContext.DocumentedTypes.Add( dtDb );
            }
            else
            {
                dtDb.Deprecated = false;

                if( dtDb.CodeFiles == null )
                    dtDb.CodeFiles = new List<CodeFile> { codeFileDb };
                else
                {
                    if( dtDb.CodeFiles.All( x => x.ID != codeFileDb.ID ) )
                        dtDb.CodeFiles.Add( codeFileDb );
                }
            }

            dtDb.FullyQualifiedName = dtFQNames[0];

            if ( !SetContainer( nodeContext, dtDb ) )
                return false;

            if( !SetNamespace( nodeContext.Node, dtDb ) )
                return false;

            dtDb.Kind = nodeContext.Node.Kind() switch
            {
                SyntaxKind.ClassDeclaration => NamedTypeKind.Class,
                SyntaxKind.InterfaceDeclaration => NamedTypeKind.Interface,
                SyntaxKind.RecordDeclaration => NamedTypeKind.Record,
                SyntaxKind.StructDeclaration => NamedTypeKind.Struct,
                _ => undefined_kind()
            };

            dtDb.Accessibility = nodeContext.Node.GetAccessibility();
            dtDb.IsAbstract = nodeContext.Node.HasChildNode(SyntaxKind.AbstractKeyword );
            dtDb.IsSealed = nodeContext.Node.HasChildNode(SyntaxKind.SealedKeyword );
            dtDb.IsStatic = nodeContext.Node.HasChildNode(SyntaxKind.StaticKeyword );

            DbContext.SaveChanges();

            ProcessTypeParameters( typeConstraints, dtDb );

            return true;

            NamedTypeKind undefined_kind()
            {
                Logger?.Error("Unsupported named type '{0}'", nodeContext.Node.Kind());

                return NamedTypeKind.Unsupported;
            }
        }

        private bool SetNamespace( SyntaxNode node, DocumentedType dtDb )
        {
            // find our containing namespace, which may not be our parent
            var curNode = node;

            while( curNode != null && !curNode.IsKind( SyntaxKind.NamespaceDeclaration ) )
            {
                curNode = curNode.Parent;
            }

            if( curNode == null )
                return true;

            if( !GetNamespace( curNode, out var theNs ) )
                return false;

            if( dtDb.Namespace == null )
                dtDb.NamespaceID = theNs!.ID;
            else dtDb.Namespace = theNs;

            return true;
        }

        private bool SetContainer(NodeContext nodeContext, DocumentedType dtDb )
        {
            if (nodeContext.Node.Parent == null)
            {
                Logger?.Error("Supplied named type declaration node has no parent");
                return false;
            }

            var parentKind = nodeContext.Node.Parent.Kind();

            return parentKind switch
            {
                SyntaxKind.NamespaceDeclaration => SetNamespaceContainer(nodeContext, dtDb),
                SyntaxKind.CompilationUnit => SetCodeFileContainer(nodeContext, dtDb),
                SyntaxKind.ClassDeclaration=>SetNamedTypeContainer(nodeContext, dtDb),
                SyntaxKind.InterfaceDeclaration => SetNamedTypeContainer(nodeContext, dtDb),
                SyntaxKind.RecordDeclaration => SetNamedTypeContainer(nodeContext, dtDb),
                SyntaxKind.StructDeclaration => SetNamedTypeContainer(nodeContext, dtDb),
                _ => unsupported( parentKind )
            };

            bool unsupported( SyntaxKind theKind )
            {
                Logger?.Error("Unsupported NamespaceDeclaration node parent kind '{0}'", nodeContext.Node.Parent.Kind());
                return false;
            }
        }

        private bool SetNamedTypeContainer( NodeContext nodeContext, DocumentedType dtDb )
        {
            if( !FullyQualifiedNames.GetName( nodeContext.Node.Parent!, out var parentFQNames ) )
            {
                Logger?.Error<string>(
                    "Could not determine fully-qualified name of parent named type node for {0}",
                    dtDb.FullyQualifiedName );

                return false;
            }

            if( parentFQNames!.Count != 1 )
            {
                Logger?.Error("Multiple alternative fully-qualified names for SyntaxNode parent");
                return false;
            }

            var ntParent = DbContext.DocumentedTypes
                .FirstOrDefault( x => x.FullyQualifiedName == parentFQNames[0] );

            if( ntParent == null )
            {
                Logger?.Error<string>(
                    "Could not find parent named type node {0}", parentFQNames[0] );

                return false;
            }

            dtDb.SetContainer( ntParent );

            return true;
        }

        private bool SetCodeFileContainer(NodeContext nodeContext, DocumentedType dtDb)
        {
            dtDb.SetNotContained();

            return true;
        }

        private bool SetNamespaceContainer( NodeContext nodeContext, DocumentedType dtDb )
        {
            if( !GetNamespace( nodeContext.Node.Parent!, out var nsParent ) )
                return false;

            dtDb.SetContainer( nsParent! );
            return true;
        }

        private bool GetNamespace( SyntaxNode node, out Namespace? result )
        {
            result = null;

            if( !node.IsKind( SyntaxKind.NamespaceDeclaration ) )
            {
                Logger?.Error("SyntaxNode is not a NamespaceDeclaration"  );
                return false;
            }

            if( !FullyQualifiedNames.GetName( node, out var nsFQNames ) )
            {
                Logger?.Error( "Could not determine fully-qualified name of NamespaceDeclaration node" );
                return false;
            }

            if( nsFQNames!.Count != 1 )
            {
                Logger?.Error("Multiple alternative containing Namespace names"  );
                return false;
            }

            result = DbContext.Namespaces.FirstOrDefault( x => x.FullyQualifiedName == nsFQNames[0] );

            if( result != null ) 
                return true;

            Logger?.Error<string>(
                "Could not find parent NamespaceDeclarationSyntax node {0}",
                nsFQNames[0] );

            return false;
        }

        private void ProcessTypeParameters( List<TypeParameterInfo> typeParameters, DocumentedType dtDb )
        {
            dtDb.TypeParameters ??= new List<TypeParameter>();

            for( var idx = 0; idx < typeParameters.Count; idx++ )
            {
                var tpDb = DbContext.TypeParameters
                    .FirstOrDefault( x => x.DefinedInID == dtDb.ID && x.Name == typeParameters[ idx ].Name );

                if( tpDb == null )
                {
                    tpDb = dtDb.ID == 0
                        ? new TypeParameter
                        {
                            DefinedIn = dtDb,
                            Name = typeParameters[idx].Name,
                            Index = idx
                        }
                        : new TypeParameter
                        {
                            DefinedInID = dtDb.ID,
                            Name = typeParameters[idx].Name,
                            Index = idx
                        };

                    DbContext.TypeParameters.Add( tpDb );
                }
                else tpDb.Index = idx;

                if( typeParameters[ idx ].TypeConstraintNode != null )
                {
                    typeParameters[ idx ].TypeConstraintNode!
                        .GetGeneralTypeConstraints( out var temp );

                    tpDb.GeneralTypeConstraints = temp;
                }

                DbContext.SaveChanges();
            }
        }
    }
}