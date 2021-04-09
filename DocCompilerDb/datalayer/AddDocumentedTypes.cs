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
    public class AddDocumentedTypes : SyntaxNodeProcessor
    {
        public static SyntaxKind[] SupportedKinds = new[]
        {
            SyntaxKind.ClassDeclaration,
            SyntaxKind.InterfaceDeclaration,
            SyntaxKind.RecordDeclaration,
            SyntaxKind.StructDeclaration
        };

        private readonly TypeParameterListFQN _tplFQN;

        public AddDocumentedTypes( 
            IFullyQualifiedNames fqNamers,
            TypeParameterListFQN tplFQN,
            DocDbContext dbContext, 
            IJ4JLogger? logger ) 
            : base( fqNamers, dbContext, logger )
        {
            _tplFQN = tplFQN;
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
            if( !Namers.GetFullyQualifiedName( nodeContext.Node, out var fqName ) )
                return false;

            if (!Namers.GetName(nodeContext.Node, out var nsName))
                return false;

            var codeFileDb = DbContext.CodeFiles
                .FirstOrDefault( x => x.FullPath == nodeContext.ScannedFile.SourceFilePath );

            if( codeFileDb == null )
            {
                Logger?.Error<string>( "Could not find CodeFile reference for '{0}'",
                    nodeContext.ScannedFile.SourceFilePath );

                return false;
            }

            var dtDb = DbContext.DocumentedTypes
                .Include(x=>x.CodeFiles)
                .Include(x=>x.TypeParameters)
                .FirstOrDefault(x => x.FullyQualifiedName== fqName);

            if( dtDb == null )
            {
                dtDb = new DocumentedType
                {
                    FullyQualifiedName = fqName!,
                    Name = nsName!,
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

            if( !SetContainer( nodeContext, dtDb ) )
                return false;

            dtDb.Kind = nodeContext.Node.Kind() switch
            {
                SyntaxKind.ClassDeclaration => NamedTypeKind.Class,
                SyntaxKind.InterfaceDeclaration => NamedTypeKind.Interface,
                SyntaxKind.RecordDeclaration => NamedTypeKind.Record,
                SyntaxKind.StructDeclaration => NamedTypeKind.Struct,
                _ => undefined_kind()
            };

            dtDb.Accessibility = GetAccessibility( nodeContext.Node );
            dtDb.IsAbstract = HasChildNode( nodeContext, SyntaxKind.AbstractKeyword );
            dtDb.IsSealed = HasChildNode( nodeContext, SyntaxKind.SealedKeyword );
            dtDb.IsStatic = HasChildNode( nodeContext, SyntaxKind.StaticKeyword );

            DbContext.SaveChanges();

            ProcessTypeParameterList( nodeContext.Node, dtDb );

            return true;
            //return UpdateDocumentedTypeUsings( dtDb, nodeContext.ScannedFile );

            NamedTypeKind undefined_kind()
            {
                Logger?.Error("Unsupported named type '{0}'", nodeContext.Node.Kind());

                return NamedTypeKind.Unsupported;
            }
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
            if( !Namers.GetFullyQualifiedName( nodeContext.Node.Parent!, out var parentFQName ) )
            {
                Logger?.Error<string>(
                    "Could not determine fully-qualified name of parent named type node for {0}",
                    dtDb.FullyQualifiedName );

                return false;
            }

            var ntParent = DbContext.DocumentedTypes
                .FirstOrDefault( x => x.FullyQualifiedName == parentFQName! );

            if( ntParent == null )
            {
                Logger?.Error<string>(
                    "Could not find parent named type node {0}", parentFQName! );

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
            if( !Namers.GetFullyQualifiedName( nodeContext.Node.Parent!, out var nsFQName ) )
            {
                Logger?.Error<string>(
                    "Could not determine fully-qualified name of parent NamespaceDeclarationSyntax node for {0}",
                    dtDb.FullyQualifiedName );

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

            dtDb.SetContainer( nsParent );
            return true;
        }

        private void ProcessTypeParameterList( SyntaxNode node, DocumentedType dtDb )
        {
            var tplNode = node.ChildNodes()
                .FirstOrDefault( x => x.IsKind( SyntaxKind.TypeParameterList ) );
            
            if( tplNode == null || !_tplFQN.GetIdentifierTokens(tplNode, out var temp ) )
                return;

            var idTokens = temp.ToList();

            var constraintNodes = node.ChildNodes()
                .Where( x => x.IsKind( SyntaxKind.TypeParameterConstraintClause ) )
                .Select( x =>
                {
                    return new
                    {
                        Name = x.ChildNodes().First( y => y.IsKind( SyntaxKind.IdentifierName ) ).ChildTokens()
                            .First( z => z.IsKind( SyntaxKind.IdentifierToken ) ).Text,
                        ConstraintNode = x
                    };
                } )
                .ToDictionary( x => x.Name, x => x.ConstraintNode );

            dtDb.TypeParameters ??= new List<TypeParameter>();

            for( var idx = 0; idx < idTokens!.Count(); idx++ )
            {
                var tpName = idTokens[ idx ].Text;

                var tpDb = DbContext.TypeParameters
                    .FirstOrDefault( x => x.DefinedInID == dtDb.ID && x.Name == tpName );

                if( tpDb == null )
                {
                    tpDb = dtDb.ID == 0
                        ? new TypeParameter
                        {
                            DefinedIn = dtDb,
                            Name = tpName,
                            Index = idx
                        }
                        : new TypeParameter
                        {
                            DefinedInID = dtDb.ID,
                            Name = tpName,
                            Index = idx
                        };

                    DbContext.TypeParameters.Add( tpDb );
                }
                else tpDb.Index = idx;

                if( !constraintNodes.ContainsKey( tpName ) )
                {
                    DbContext.SaveChanges();
                    continue;
                }

                var constraintNode = constraintNodes[ tpName ];

                if( GetGeneralTypeConstraints( constraintNode, out var generalConstraints ) )
                    tpDb.GeneralTypeConstraints = generalConstraints;

                DbContext.SaveChanges();
            }
        }

        //private bool UpdateDocumentedTypeUsings( DocumentedType dtDb, IScannedFile scannedFile )
        //{
        //    var cfDb = DbContext.CodeFiles
        //        .FirstOrDefault( x => x.FullPath == scannedFile.SourceFilePath );

        //    if( cfDb == null )
        //    {
        //        Logger?.Error<string>("Could not find CodeFile for DocumentedType '{0}'", dtDb.FullyQualifiedName);

        //        return false;
        //    }

        //    // ascend through any containing DocumentedTypes
        //    var curDT = DbContext.DocumentedTypes
        //        .FirstOrDefault( x => x.ID == dtDb.ContainingDocumentedTypeID );

        //    DbContext.DocumentedTypeUsings
        //        .RemoveRange(
        //            DbContext.DocumentedTypeUsings.Where( x => x.DocumentedTypeID == dtDb.ID )
        //        );

        //    var idx = 0;

        //    while( curDT != null )
        //    {
        //        DbContext.DocumentedTypeUsings.Add( new DocumentedTypeUsing
        //        {
        //            DocumentedTypeID = dtDb.ID,
        //            UsingText = curDT.FullyQualifiedName,
        //            Index = idx
        //        } );

        //        idx++;

        //        var parentDT = DbContext.DocumentedTypes
        //            .FirstOrDefault( x => x.ID == curDT.ContainingDocumentedTypeID );

        //        if( parentDT == null )
        //        {
        //            Logger?.Error<int, string>( "Could not find containing DocumentedType (ID = {0}) for {1} ",
        //                curDT.ID,
        //                curDT.FullyQualifiedName );

        //            break;
        //        }

        //        curDT = parentDT;
        //    }

        //    curDT ??= dtDb;

        //    // ascend through any containing Namespaces, also adding any Using statements they may harbor
        //    var curNS = DbContext.Namespaces
        //        .FirstOrDefault( x => x.ID == curDT.ContainingNamespaceID );

        //    while( curNS != null )
        //    {
        //        DbContext.DocumentedTypeUsings.Add( new DocumentedTypeUsing
        //        {
        //            DocumentedTypeID = dtDb.ID,
        //            UsingText = curNS.FullyQualifiedName,
        //            Index = idx
        //        } );

        //        idx++;

        //        var curID = curNS.ID;

        //        foreach( var curNsRef in DbContext.NamespaceUsings
        //            .Where(x=>x.CodeFileID == cfDb.ID  )
        //            .Include(x=>x.InScopeNamespaces  )
        //            .Where( x => x.NamespaceReferences!.Any( y => y.CodeFileID == curID && y.CodeFileID == cfDb.ID ) ) )
        //        {
        //            DbContext.DocumentedTypeUsings.Add( new DocumentedTypeUsing
        //            {
        //                DocumentedTypeID = dtDb.ID,
        //                UsingText = curNsRef.Name,
        //                Index = idx
        //            } );

        //            idx++;
        //        }

        //        var parentNS = DbContext.Namespaces
        //            .FirstOrDefault( x => x.ID == curNS.ContainingNamespaceID );

        //        curNS = parentNS;
        //    }

        //    // add any code-file usings
        //    foreach( var curUsing in DbContext.Usings
        //        .Where( x => x.CodeFiles!.Any( y => y.FullPath == scannedFile.SourceFilePath ) ) )
        //    {
        //        DbContext.DocumentedTypeUsings.Add( new DocumentedTypeUsing
        //        {
        //            DocumentedTypeID = dtDb.ID,
        //            UsingText = curUsing.Name,
        //            Index = idx
        //        } );

        //        idx++;
        //    }

        //    DbContext.SaveChanges();

        //    return true;
        //}
    }
}