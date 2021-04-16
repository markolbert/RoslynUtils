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

using System;
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
        private readonly List<ITypeResolver> _typeResolvers;
        private readonly IJ4JLogger? _logger;

        public TypeReferenceResolver(
            IEnumerable<ITypeResolver> typeResolvers,
            TopologicalSortFactory tsFactory,
            DocDbContext dbContext,
            IJ4JLogger? logger
        )
        {
            _dbContext = dbContext;

            if( !tsFactory.CreateSortedList( typeResolvers, out var temp ) )
                throw new ArgumentException( "Could not topologically sort collection of ITypeResolver" );

            _typeResolvers = temp!;
            
            _logger = logger;
            _logger?.SetLoggedType( GetType() );
        }

        public bool Resolve( 
            ITypeNodeAnalyzer analyzer, 
            DocumentedType dtContextDb, 
            TypeReference? parentRef,
            out TypeReference? result )
        {
            result = null;

            if( analyzer.IsValid )
                return ResolveInternal( analyzer, analyzer.RootTypeReference!, dtContextDb, parentRef, out result );

            _logger?.Error( "Attempting to resolve a type based on an invalid analysis" );

            return false;
        }

        private bool ResolveInternal( 
            ITypeNodeAnalyzer analyzer, 
            TypeReferenceInfo typeInfo, 
            NamedType ntContextDb, 
            TypeReference? parentRef,
            out TypeReference? result )
        {
            result = null;

            foreach( var typeResolver in _typeResolvers )
            {
                if( !typeResolver.FindType( analyzer, typeInfo, ntContextDb, out var ntDb ) )
                    continue;

                result = new TypeReference
                {
                    Index = analyzer.RootTypeReference!.Index,
                    ReferencedTypeRank = analyzer.RootTypeReference.Rank
                };

                _dbContext.TypeReferences.Add( result );

                if( ntDb!.ID == 0 )
                    result.ReferencedType = ntDb;
                else result.ReferencedTypeID = ntDb.ID;

                if( parentRef == null ) 
                    return process_children( result );

                if( parentRef.ID == 0 )
                    result.ParentReference = parentRef;
                else result.ParentReferenceID = parentRef.ID;

                return true;
            }

            _logger?.Error("Could not resolve type");
            
            return false;

            bool process_children(TypeReference curParent)
            {
                foreach (var argInfo in analyzer.RootTypeReference!.Arguments)
                {
                    if (!ResolveInternal(analyzer, argInfo, ntContextDb, curParent, out _))
                        return false;
                }

                return true;
            }
        }

        //private bool ResolveInternal( TypeReferenceInfo typeInfo, NamedType ntContextDb, TypeReference? parentRef, out TypeReference? result )
        //{
        //    result = null;

        //    NamedType? namedType = null;

        //    if( FindDocumentedType( typeInfo!, ntContextDb, out var temp ) )
        //        namedType = temp;
        //    else
        //    {
        //        if( FindLocalType( typeInfo!, ntContextDb, out var temp2 ) )
        //            namedType = temp2;
        //        else
        //        {
        //            if( FindExternalType( typeInfo!, ntContextDb, out var temp3 ) )
        //                namedType = temp3;
        //        }
        //    }

        //    if( namedType == null )
        //        return false;

        //    result = new TypeReference
        //    {
        //        Index = typeInfo.Index,
        //        ReferencedTypeRank = typeInfo.Rank
        //    };

        //    _dbContext.TypeReferences.Add( result );

        //    if( namedType.ID == 0 )
        //        result.ReferencedType = namedType;
        //    else result.ReferencedTypeID = namedType.ID;

        //    if( parentRef == null ) 
        //        return process_children( result );

        //    if( parentRef.ID == 0 )
        //        result.ParentReference = parentRef;
        //    else result.ParentReferenceID = parentRef.ID;

        //    return process_children( result );

        //    bool process_children( TypeReference curParent )
        //    {
        //        foreach( var argInfo in typeInfo.Arguments )
        //        {
        //            if( !ResolveInternal( argInfo, ntContextDb, curParent, out _ ) )
        //                return false;
        //        }

        //        return true;
        //    }
        //}

        //private List<NamespaceContext> GetNamespaceContexts( NamedType ntDb )
        //{
        //    // build the list of namespaces which define the context within which we'll be
        //    // searching for a NamedType
        //    var retVal = new List<NamespaceContext>( _cfNsContexts );

        //    if( ntDb is DocumentedType dtContextDb)
        //        retVal = dtContextDb.GetNamespaceContext( retVal );

        //    return retVal;
        //}

        //private bool FindDocumentedType( TypeReferenceInfo typeInfo, NamedType ntDb, out DocumentedType? result )
        //{
        //    result = null;

        //    foreach( var nsContext in GetNamespaceContexts(ntDb) )
        //    {
        //        result = _dbContext.DocumentedTypes
        //            .Include( x => x.TypeParameters )
        //            .FirstOrDefault( x =>
        //                x.FullyQualifiedNameWithoutTypeParameters == $"{nsContext.NamespaceName}.{typeInfo.Name}"
        //                && ( x.TypeParameters == null && !typeInfo.Arguments.Any()
        //                     || x.TypeParameters != null && x.TypeParameters.Count == typeInfo.Arguments.Count
        //                ) );

        //        if( result != null )
        //            return true;
        //    }

        //    return false;
        //}

        //private bool FindExternalType( TypeReferenceInfo typeInfo, NamedType ntDb, out ExternalType? result )
        //{
        //    result = null;

        //    var nsContexts = GetNamespaceContexts( ntDb );

        //    var possibleNamedTypes = _dbContext.ExternalTypes
        //        .Include( x => x.PossibleNamespaces )
        //        .Where( x => x.Name == typeInfo.Name 
        //                     && x.NumTypeParameters == typeInfo.Arguments.Count );

        //    if( !possibleNamedTypes.Any() )
        //    {
        //        result = _createIfMissing ? CreateExternalType( typeInfo, nsContexts ) : null;

        //        return result != null;
        //    }

        //    foreach( var extTypeDb in possibleNamedTypes )
        //    {
        //        if( !( extTypeDb.PossibleNamespaces?
        //            .Any( x => nsContexts.Any( y => y.NamespaceName == x.Name ) ) ?? false ) ) 
        //            continue;

        //        result = extTypeDb;

        //        return true;
        //    }

        //    // if we get here there were no matches among the ExternalTypes in the database
        //    result = _createIfMissing ? CreateExternalType( typeInfo, nsContexts ) : null;

        //    return result != null;
        //}

        //private ExternalType CreateExternalType( TypeReferenceInfo typeInfo, List<NamespaceContext> nsContexts )
        //{
        //    var retVal = new ExternalType
        //    {
        //        Name = typeInfo.Name,
        //        Accessibility = Accessibility.ExternallyDefined,
        //        NumTypeParameters = typeInfo.Arguments.Count,
        //        PossibleNamespaces = nsContexts.Select( x => x.Namespace ).ToList()
        //    };

        //    _dbContext.ExternalTypes.Add( retVal );

        //    return retVal;
        //}
    }
}