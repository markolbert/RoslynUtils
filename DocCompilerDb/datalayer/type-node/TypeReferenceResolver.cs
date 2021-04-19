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
        private readonly TypeResolvers _typeResolvers;
        private readonly IJ4JLogger? _logger;

        public TypeReferenceResolver(
            TypeResolvers typeResolvers,
            DocDbContext dbContext,
            IJ4JLogger? logger
        )
        {
            _typeResolvers = typeResolvers;
            _dbContext = dbContext;

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

            if( !_typeResolvers.FindType( analyzer, typeInfo, ntContextDb, out var resolvedType ) )
            {
                _logger?.Error<string>( "Could not resolve type for '{0}'", typeInfo.Name );
                return false;
            }

            result = new TypeReference
            {
                ReferencedTypeRank = analyzer.RootTypeReference!.Rank
            };

            _dbContext.TypeReferences.Add( result );

            if( resolvedType!.ID == 0 )
                result.ReferencedType = resolvedType;
            else result.ReferencedTypeID = resolvedType.ID;

            if( parentRef == null )
                return process_children( result );

            if( parentRef.ID == 0 )
                result.ParentReference = parentRef;
            else result.ParentReferenceID = parentRef.ID;

            return true;

            bool process_children( TypeReference curParent )
            {
                foreach( var argInfo in analyzer.RootTypeReference!.Arguments )
                {
                    if( !ResolveInternal( analyzer, argInfo, ntContextDb, curParent, out _ ) )
                        return false;
                }

                return true;
            }
        }
    }
}