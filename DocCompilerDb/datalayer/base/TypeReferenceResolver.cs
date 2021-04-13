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
using System.Reflection.Metadata;
using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.DotNet.PlatformAbstractions;
using Microsoft.EntityFrameworkCore;

namespace J4JSoftware.DocCompiler
{
    public class TypeReferenceResolver : ITypeReferenceResolver
    {
        private readonly DocDbContext _dbContext;
        private readonly List<NamespaceContext> _cfNsContexts = new();

        private readonly IJ4JLogger? _logger;

        private CodeFile? _codeFile;
        private bool _createIfMissing;

        public TypeReferenceResolver(
            DocDbContext dbContext,
            IJ4JLogger? logger
        )
        {
            _dbContext = dbContext;
            
            _logger = logger;
            _logger?.SetLoggedType( GetType() );
        }

        public bool Resolve( 
            SyntaxNode typeNode, 
            DocumentedType dtContextDb, 
            IScannedFile scannedFile,
            out TypeReference? result,
            bool createIfMissing = true )
        {
            _createIfMissing = createIfMissing;
            result = null;

            if( !typeNode.IsKind( SyntaxKind.SimpleBaseType ) )
            {
                _logger?.Error( "SyntaxNode is not a SimpleBaseType" );
                return false;
            }

            _codeFile = _dbContext.CodeFiles.FirstOrDefault( x => x.FullPath == scannedFile.SourceFilePath );

            if( _codeFile == null )
            {
                _logger?.Error<string>( "Could not find CodeFile entity in database for DocumentedType '{0}'",
                    dtContextDb.FullyQualifiedName );

                return false;
            }

            _cfNsContexts.Clear();
            _cfNsContexts.AddRange( _codeFile.GetNamespaceContext() );

            if( !GetTypeInfo( typeNode, out var rootTypeInfo ) )
                return false;

            if( !ResolveInternal( rootTypeInfo!, dtContextDb ) )
                return false;

            result = new TypeReference { ReferencedTypeRank = rootTypeInfo!.Rank };

            if( rootTypeInfo.DbEntity!.ID == 0 )
                result.ReferencedType = rootTypeInfo.DbEntity;
            else result.ReferencedTypeID = rootTypeInfo.DbEntity.ID;

            return true;
        }

        private bool GetTypeInfo( SyntaxNode node, out TypeInfo? result )
        {
            switch( node.Kind() )
            {
                case SyntaxKind.SimpleBaseType:
                    return GetTypeInfo( node.ChildNodes().First(), out result );
                
                case SyntaxKind.IdentifierName:
                    result = new TypeInfo( node );
                    return true;

                case SyntaxKind.PredefinedType:
                    result = new TypeInfo( node ) { IsPredefined = true };
                    return true;

                case SyntaxKind.ArrayType:
                    // the first child node is supposed to be the "basic type name"
                    if( !GetTypeInfo( node.ChildNodes().First(), out result ) )
                        return false;

                    result!.Rank = node.DescendantNodes()
                        .Count( x => x.IsKind( SyntaxKind.OmittedArraySizeExpression ) );

                    return true;

                case SyntaxKind.GenericName:
                    result = new TypeInfo( node.ChildTokens()
                        .First( x => x.IsKind( SyntaxKind.IdentifierToken ) )
                        .Text );

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
                    
                default:
                    result = null;
                    _logger?.Error("Unsupported SyntaxNode '{0}'", node.Kind()  );

                    return false;
            }
        }

        private bool ResolveInternal( TypeInfo typeInfo, NamedType ntContextDb )
        {
            // build the list of namespaces which define the context within which we'll be
            // searching for a NamedType
            var nsContexts = new List<NamespaceContext>( _cfNsContexts );

            if( ntContextDb is DocumentedType dtContextDb)
                nsContexts = dtContextDb.GetNamespaceContext( nsContexts );

            if (FindDocumentedType(typeInfo!, nsContexts, out var temp))
                typeInfo.DbEntity = temp;
            else
            {
                if( FindExternalType( typeInfo!, nsContexts, out var temp2 ) )
                    typeInfo.DbEntity = temp2;
            }

            return typeInfo.DbEntity != null;
        }

        private bool FindDocumentedType(
            TypeInfo typeInfo,
            List<NamespaceContext> nsContexts,
            out DocumentedType? result )
        {
            result = null;

            foreach( var nsContext in nsContexts )
            {
                foreach( var dtMatch in _dbContext.DocumentedTypes
                    .Include( x => x.TypeParameters )
                    .Where( x => x.FullyQualifiedNameWithoutTypeParameters == $"{nsContext.NamespaceName}.{typeInfo.Name}"
                                 && ( x.TypeParameters == null && !typeInfo.Arguments.Any() 
                                      || x.TypeParameters != null && x.TypeParameters.Count == typeInfo.Arguments.Count
                                 ) ) )
                {
                    foreach( var argInfo in typeInfo.Arguments )
                    {
                        if( !ResolveInternal( argInfo, dtMatch ) )
                            return false;
                    }

                    result = dtMatch;

                    return true;
                }
            }

            return false;
        }

        private bool FindExternalType( TypeInfo typeInfo, List<NamespaceContext> nsContexts, out ExternalType? result )
        {
            result = null;

            var possibleNamedTypes = _dbContext.ExternalTypes
                .Include( x => x.PossibleNamespaces )
                .Where( x => x.Name == typeInfo.Name 
                             && x.NumTypeParameters == typeInfo.Arguments.Count );

            if( !possibleNamedTypes.Any() )
            {
                result = _createIfMissing ? CreateExternalType( typeInfo, nsContexts ) : null;

                return result != null;
            }

            foreach( var extTypeDb in possibleNamedTypes )
            {
                if( !( extTypeDb.PossibleNamespaces?
                    .Any( x => nsContexts.Any( y => y.NamespaceName == x.Name ) ) ?? false ) ) 
                    continue;

                result = extTypeDb;

                return true;
            }

            // if we get here there were no matches among the ExternalTypes in the database
            result = _createIfMissing ? CreateExternalType( typeInfo, nsContexts ) : null;

            return result != null;
        }

        private ExternalType CreateExternalType( TypeInfo typeInfo, List<NamespaceContext> nsContexts )
        {
            var retVal = new ExternalType
            {
                Name = typeInfo.Name,
                Accessibility = Accessibility.ExternallyDefined,
                NumTypeParameters = typeInfo.Arguments.Count,
                PossibleNamespaces = nsContexts.Select( x => x.Namespace ).ToList()
            };

            _dbContext.ExternalTypes.Add( retVal );

            return retVal;
        }
    }
}