#region license

// Copyright 2021 Mark A. Olbert
// 
// This library or program 'DocCompilerDb' is free software: you can redistribute it
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
using CSharpExtensions = Microsoft.CodeAnalysis.CSharpExtensions;

namespace J4JSoftware.DocCompiler
{
    public class TypeNodeAnalyzer : ITypeNodeAnalyzer
    {
        public static SyntaxKind[] SupportedKinds = { SyntaxKind.SimpleBaseType, SyntaxKind.TypeConstraint };

        private readonly DocDbContext _dbContext;
        private readonly IJ4JLogger? _logger;

        public TypeNodeAnalyzer(
            DocDbContext dbContext,
            IJ4JLogger? logger
        )
        {
            _dbContext = dbContext;

            _logger = logger;
            _logger?.SetLoggedType( GetType() );
        }

        public bool IsValid => DocumentedTypeContext != null;
        public DocumentedType? DocumentedTypeContext { get; private set; }
        public bool CreateIfMissing { get; private set; }
        public List<NamespaceContext>? CodeFileNamespaceContexts { get; private set; }
        public List<TypeParameter>? TypeParameters { get; private set; }
        public TypeReferenceInfo? RootTypeReference { get; private set; }

        public virtual bool Analyze(
            SyntaxNode typeNode,
            DocumentedType dtContextDb,
            IScannedFile scannedFile,
            bool createIfMissing = true )
        {
            DocumentedTypeContext = null;
            CreateIfMissing = false;
            CodeFileNamespaceContexts = null;
            TypeParameters = null;
            RootTypeReference = null;

            if( SupportedKinds.All( x => !typeNode.IsKind( x ) ) )
            {
                _logger?.Error( "SyntaxNode is not a supported type of node for TypeReference resolution" );
                return false;
            }

            var codeFile = _dbContext.CodeFiles.FirstOrDefault( x => x.FullPath == scannedFile.SourceFilePath );

            if( codeFile == null )
            {
                _logger?.Error<string>( "Could not find CodeFile entity in database for NamedType '{0}'",
                    dtContextDb.FullyQualifiedName );

                return false;
            }

            DocumentedTypeContext = dtContextDb;
            CreateIfMissing = createIfMissing;
            CodeFileNamespaceContexts = codeFile.GetNamespaceContext();
            
            if( dtContextDb.NumTypeParameters > 0 )
            {
                if( dtContextDb.TypeParameters == null )
                    _dbContext.Entry( dtContextDb )
                        .Collection( x => x.TypeParameters )
                        .Load();
            }

            TypeParameters = dtContextDb.TypeParameters?.ToList() ?? new List<TypeParameter>();

            if( !GetTypeInfo( typeNode, out var temp ) )
                return false;

            RootTypeReference = temp;

            return true;
        }

        protected virtual bool GetTypeInfo( SyntaxNode node, out TypeReferenceInfo? result )
        {
            result = null;

            return node.Kind() switch
            {
                SyntaxKind.SimpleBaseType => GetTypeInfo( node.ChildNodes().First(), out result ),
                SyntaxKind.TypeConstraint =>GetTypeInfo( node.ChildNodes().First(), out result ),
                SyntaxKind.IdentifierName => ProcessIdentifierName(node, out result),
                SyntaxKind.PredefinedType=> ProcessPredefinedType(node, out result),
                SyntaxKind.ArrayType=>ProcessArrayType(node, out result),
                SyntaxKind.GenericName=>ProcessGenericName(node, out result),
                _ => unsupported()
            };

            bool unsupported()
            {
                _logger?.Error("Unsupported SyntaxNode '{0}'", node.Kind()  );
                return false;
            }
        }

        protected virtual bool ProcessIdentifierName( SyntaxNode node, out TypeReferenceInfo? result )
        {
            result = null;

            if( !node.IsKind( SyntaxKind.IdentifierName ) )
                return false;

            result = new TypeReferenceInfo( node );
            return true;
        }

        protected virtual bool ProcessPredefinedType( SyntaxNode node, out TypeReferenceInfo? result )
        {
            result = null;

            if( !node.IsKind( SyntaxKind.PredefinedType ) )
                return false;

            result = new TypeReferenceInfo( node ) { IsPredefined = true };
            return true;
        }

        protected virtual bool ProcessArrayType( SyntaxNode node, out TypeReferenceInfo? result )
        {
            result = null;

            if( !node.IsKind( SyntaxKind.ArrayType ) )
                return false;

            // the first child node is supposed to be the "basic type name"
            if( !GetTypeInfo( node.ChildNodes().First(), out result ) )
                return false;

            result!.Rank = node.DescendantNodes()
                .Count( x => x.IsKind( SyntaxKind.OmittedArraySizeExpression ) );

            return true;
        }

        protected virtual bool ProcessGenericName( SyntaxNode node, out TypeReferenceInfo? result )
        {
            result = null;

            if( !node.IsKind( SyntaxKind.GenericName ) )
                return false;

            result = new TypeReferenceInfo( node.ChildTokens()
                .First( x => x.IsKind( SyntaxKind.IdentifierToken ) ) );

            if( !node.GetChildNode( SyntaxKind.TypeArgumentList, out var talNode ) )
                _logger?.Error("GenericName node does not contain a TypeArgumentList node"  );
            else
            {
                foreach( var taNode in talNode!.ChildNodes() )
                {
                    if( !GetTypeInfo(taNode, out var taTypeInfo))
                        return false;

                    result.AddChild(taTypeInfo!);
                }
            }

            return true;
        }
    }
}