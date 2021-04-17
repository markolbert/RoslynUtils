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
            {
                if( analyzer.RootTypeReference!.IsTuple )
                    return ResolveTuple( analyzer, analyzer.RootTypeReference!, dtContextDb, parentRef, out result ); 
                
                return ResolveInternal( analyzer, analyzer.RootTypeReference!, dtContextDb, parentRef, out result );
            }

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
                    ReferencedTypeRank = analyzer.RootTypeReference!.Rank
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

        private bool ResolveTuple(
            ITypeNodeAnalyzer analyzer,
            TypeReferenceInfo typeInfo,
            NamedType ntContextDb,
            TypeReference? parentRef,
            out TypeReference? result )
        {
            result = null;

            var elementReferences = new List<TypeReference>();

            TypeReference? childTR = null;

            foreach( var elementTRI in analyzer.RootTypeReference!.Arguments )
            {
                if( elementTRI.IsTuple )
                {
                    if( !ResolveTuple( analyzer, elementTRI, ntContextDb, parentRef, out childTR ) )
                    {
                        _logger?.Error<string>( "Could not resolve TupleElement type '{0}'", elementTRI.Name );
                        return false;
                    }
                }
                else
                {
                    if( !ResolveInternal( analyzer, elementTRI, ntContextDb, parentRef, out childTR ) )
                    {
                        _logger?.Error<string>( "Could not resolve type '{0}'", elementTRI.Name );
                        return false;
                    }
                }

                elementReferences.Add( childTR! );
            }

            return true;
        }
    }
}