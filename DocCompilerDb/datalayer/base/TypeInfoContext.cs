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
using Microsoft.EntityFrameworkCore;

namespace J4JSoftware.DocCompiler
{
    public class TypeInfoContext : ITypeFinder
    {
        private readonly DocDbContext _dbContext;
        private readonly List<NamespaceContext> _cfNsContexts = new();

        private readonly IJ4JLogger? _logger;

        private CodeFile? _codeFile;
        private bool _createIfMissing;

        public TypeInfoContext(
            DocDbContext dbContext,
            IJ4JLogger? logger
        )
        {
            _dbContext = dbContext;
            
            _logger = logger;
            _logger?.SetLoggedType( GetType() );
        }

        public bool IsValid { get; private set; }
        public TypeInfo Root { get; } = new();

        public bool Resolve( SyntaxNode typeNode, DocumentedType dtContextDb, IScannedFile scannedFile, bool createIfMissing = true )
        {
            _createIfMissing = createIfMissing;

            _codeFile = _dbContext.CodeFiles.FirstOrDefault( x => x.FullPath == scannedFile.SourceFilePath );

            if( _codeFile == null )
            {
                _logger?.Error<string>( "Could not find CodeFile entity in database for DocumentedType '{0}'",
                    dtContextDb.FullyQualifiedName );

                return false;
            }

            _cfNsContexts.Clear();
            _cfNsContexts.AddRange( _codeFile.GetNamespaceContext() );

            if( !PopulateTypeInfo( typeNode, Root ) )
                return false;

            if( Root.Arguments.Count != 1 )
            {
                _logger?.Error("Invalid TypeInfo derived from scanning SyntaxNode");
                return false;
            }

            IsValid = ResolveInternal( Root.Arguments.First(), dtContextDb );

            return IsValid;
        }

        private bool PopulateTypeInfo( SyntaxNode node, TypeInfo typeInfo )
        {
            if( !node.GetChildNode( out var typeNode, 
                SyntaxKind.GenericName, SyntaxKind.IdentifierName, SyntaxKind.PredefinedType ) )
            {
                _logger?.Error( "Type container node contains neither a GenericName, an IdentifierName node nor a PredefinedType node" );
                return false;
            }

            var childTypeInfo = typeInfo.AddChild( typeNode!.ChildTokens().First().Text );

            if( !typeNode!.GetChildNode( SyntaxKind.TypeParameterList, out var tplNode ) )
                return true;

            return tplNode!.ChildNodes().All( childNode => PopulateTypeInfo( childNode, childTypeInfo ) );
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

            foreach( var fqInfo in nsContexts )
            {
                foreach( var dtMatch in _dbContext.DocumentedTypes
                    .Where( x => x.FullyQualifiedName.StartsWith( fqInfo.NamespaceName ) )
                )
                {
                    // can't test for this match in EF Core LINQ because it can't be translated
                    if( !dtMatch.FullyQualifiedName.Equals( $"{fqInfo.NamespaceName}.{typeInfo.Name}" ) )
                        continue;

                    if( dtMatch.TypeParameters == null )
                    {
                        if( typeInfo.Arguments.Count != 0 )
                            continue;

                        result = dtMatch;
                        return true;
                    }

                    if( typeInfo.Arguments.Count != dtMatch.TypeParameters.Count )
                        continue;

                    // recurse over all the arguments to ensure they have type entities
                    // in the database
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

            var sameNameTypes = _dbContext.ExternalTypes
                .Include( x => x.PossibleNamespaces )
                .Include( x => x.TypeArguments )
                .Where( x => x.Name == typeInfo.Name
                             && ( x.TypeArguments == null && typeInfo.Arguments.Count == 0
                                  || x.TypeArguments != null && x.TypeArguments.Count == typeInfo.Arguments.Count )
                );

            if( !sameNameTypes.Any() )
            {
                result = _createIfMissing ? CreateExternalType( typeInfo, nsContexts ) : null;

                return result != null;
            }

            foreach( var extTypeDb in sameNameTypes )
            {
                if( extTypeDb.PossibleNamespaces?
                    .Any( x => nsContexts.Any( y => y.NamespaceName == x.Name ) ) ?? false )
                {
                    // there's a match on the existing possible namespaces, so ensure 
                    // any type arguments also exist
                    foreach( var argInfo in typeInfo.Arguments )
                    {
                        if( !ResolveInternal( argInfo, extTypeDb ) )
                            return false;
                    }

                    result = extTypeDb;

                    return true;
                }

                // there's a match on the type name but not the possible namespaces so add the current
                // namespace to the list of possibles and return the type
                var possibleNS = extTypeDb.PossibleNamespaces?.ToList() ?? new List<Namespace>();
                possibleNS.AddRange( nsContexts.Select( x => x.Namespace ) );

                extTypeDb.PossibleNamespaces = possibleNS.Distinct( Namespace.FullyQualifiedNameComparer ).ToList();

                return extTypeDb;
            }

            // if we get here there were no matches among the ExternalTypes in the database
            return _createIfMissing ? CreateExternalType( typeInfo, nsContexts ) : null;
        }

        private ExternalType CreateExternalType( TypeInfo typeInfo, List<NamespaceContext> nsContexts )
        {
            var retVal = new ExternalType
            {
                Name = typeInfo.Name,
                PossibleNamespaces = nsContexts.Select( x => x.Namespace ).ToList()
            };

            _dbContext.ExternalTypes.Add( retVal );

            return retVal;
        }
    }
}