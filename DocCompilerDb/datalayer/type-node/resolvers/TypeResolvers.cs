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
using J4JSoftware.Logging;
using Microsoft.EntityFrameworkCore;

namespace J4JSoftware.DocCompiler
{
    public class TypeResolvers
    {
        private readonly List<ITypeResolver> _typeResolvers;
        private readonly DocDbContext _dbContext;
        private readonly IJ4JLogger? _logger;

        private ITypeNodeAnalyzer? _analyzer;

        public TypeResolvers(
            IEnumerable<ITypeResolver> typeResolvers,
            DocDbContext dbContext,
            TopologicalSortFactory topoSort,
            IJ4JLogger? logger
        )
        {
            _dbContext = dbContext;

            if( !topoSort.CreateSortedList( typeResolvers, out var sorted ) )
                throw new ArgumentException( "Could not topologically sort IEnumerable<ITypeResolver>" );

            _typeResolvers = sorted!;

            _logger = logger;
            _logger?.SetLoggedType( GetType() );
        }

        public bool FindType( 
            ITypeNodeAnalyzer analyzer, 
            TypeReferenceInfo typeInfo, 
            NamedType ntDb,
            out NamedType? result )
        {
            _analyzer = analyzer;

            // tuples are handled differently because they're composed of multiple types
            return FindTypeOrTuple( typeInfo, ntDb, out result );
        }

        private bool FindTypeOrTuple( TypeReferenceInfo typeInfo, NamedType ntDb, out NamedType? result )
        {
            result = null;

            if( typeInfo.IsTuple )
            {
                if( !FindTupleType( typeInfo, ntDb, out var tupleResult ) ) 
                    return false;

                result = tupleResult;
                return true;
            }

            foreach( var resolver in _typeResolvers )
            {
                if( resolver.FindType( _analyzer!, typeInfo, ntDb, out result ) )
                    return true;
            }

            _logger?.Error( "Could not resolve TypeReferenceInfo" );

            return false;
        }

        private bool FindTupleType( TypeReferenceInfo typeInfo, NamedType ntDb, out TupleType? result )
        {
            result = null;

            if( !typeInfo.IsTuple )
                return false;

            var elementTypes = new List<NamedType>();

            // tuples are defined by having the same list of types
            var tuples = _dbContext.TupleTypes
                .Include( x => x.TupleElements )
                .Include( x => x.TupleElements!.Select( y => y.ReferencedType ) )
                .Where( x => x.TupleElements != null && x.TupleElements.Count == typeInfo.Arguments.Count )
                .ToList();

            foreach( var tuple in tuples )
            {
                var elements = tuple.TupleElements!.ToList();
                var allMatch = true;

                for( var idx = 0; idx < elements.Count; idx++ )
                {
                    if( !FindTypeOrTuple( typeInfo.Arguments[ idx ], ntDb, out var elementType ) )
                    {
                        _logger?.Error<string>( "Could not resolve tuple element '{0}'",
                            typeInfo.Arguments[ idx ].Name );
                        return false;
                    }

                    elementTypes.Add( elementType! );

                    // we enforce match on argument name because sometimes the argument
                    // name is important in context
                    allMatch &= elements[ idx ].Name == typeInfo.Name;

                    // how we match type names depends on how much "type name" is available
                    if( elements[ idx ].ReferencedType is DocumentedType dtRefDb
                        && elementType is DocumentedType dtResolvedDb )
                        allMatch &= dtResolvedDb.FullyQualifiedName == dtRefDb.FullyQualifiedName;
                    else allMatch &= elements[ idx ].ReferencedType.Name == elementType!.Name;
                }

                if( !allMatch ) 
                    continue;

                result = tuple;

                return true;
            }

            if( !_analyzer!.CreateIfMissing )
                return false;

            result = new TupleType { Name = typeInfo.Name };

            _dbContext.TupleTypes.Add( result );

            for( int idx = 0; idx < elementTypes.Count; idx++ )
            {
                var newTupleElement = new TupleElement
                {
                    TupleType = result,
                    Index = idx,
                    Name = typeInfo.Arguments[ idx ].Name,
                    ReferencedTypeRank = typeInfo.Arguments[ idx ].Rank
                };

                if( elementTypes[ idx ].ID == 0 )
                    newTupleElement.ReferencedType = elementTypes[ idx ];
                else newTupleElement.ReferencedTypeID = elementTypes[ idx ].ID;

                _dbContext.TupleElements.Add( newTupleElement );
            }

            _dbContext.SaveChanges();

            return true;
        }
    }
}