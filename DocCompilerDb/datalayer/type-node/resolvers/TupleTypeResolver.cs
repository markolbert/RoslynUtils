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
using Microsoft.EntityFrameworkCore;

namespace J4JSoftware.DocCompiler
{
    [TopologicalPredecessor(typeof(DocumentedTypeResolver))]
    public class TupleTypeResolver : TypeResolver<TupleType>
    {
        public TupleTypeResolver( 
            DocDbContext dbContext, 
            IJ4JLogger? logger ) 
            : base( dbContext, logger )
        {
        }

        protected override bool FindTypeInternal( TypeReferenceInfo typeInfo, NamedType ntDb, out TupleType? result )
        {
            result = null;

            if( !typeInfo.IsTuple )
                return false;

            var elementTypes = new List<NamedType>();

            // tuples are defined by having the same list of types...but we also include the
            // argument names in the comparison since they sometimes have significant in context
            var tuples = DbContext.TupleTypes
                .Include( x => x.TupleElements )
                .Where( x => x.TupleElements != null && x.TupleElements.Count == typeInfo.Arguments.Count )
                .ToList();

            TupleType? tupleDb = null;

            foreach( var tuple in tuples )
            {
                var allMatch = true;
                var elements = tuple.TupleElements!.ToList();

                for( var idx = 0; idx < elements.Count; idx++ )
                {

                    if( elements[ idx ].Name != typeInfo.Arguments[ idx ].Name )
                    {
                        allMatch = false;
                        break;
                    }
                }

                if( !allMatch ) 
                    continue;

                tupleDb = tuple;
                break;
            }

            return true;
        }
    }
}