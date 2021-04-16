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

using System.Linq;
using J4JSoftware.Logging;
using Microsoft.EntityFrameworkCore;

namespace J4JSoftware.DocCompiler
{
    [TopologicalPredecessor(typeof(DocumentedTypeResolver))]
    public class LocalTypeResolver : TypeResolver<LocalType>
    {
        public LocalTypeResolver( 
            DocDbContext dbContext, 
            IJ4JLogger? logger ) 
            : base( dbContext, logger )
        {
        }

        protected override bool FindTypeInternal( TypeReferenceInfo typeInfo, NamedType ntDb, out LocalType? result )
        {
            result = null;

            if( ntDb is not DocumentedType dtDb )
                return false;

            if( dtDb.TypeParameters == null )
                DbContext.Entry( dtDb )
                    .Collection( x => x.TypeParameters )
                    .Load();

            if( !dtDb.TypeParameters?.Any() ?? false )
                return false;

            var tpDb = dtDb.TypeParameters!.FirstOrDefault( x => x.Name == typeInfo.Name );

            if( tpDb == null )
                return false;

            result = DbContext.LocalTypes.FirstOrDefault( x =>
                x.DeclaringTypeID == dtDb.ID && x.TypeParameterIndex == tpDb.Index );

            if( result != null )
                return true;

            result = new LocalType
            {
                Name = typeInfo.Name, 
                TypeParameterIndex = tpDb.Index 

            };

            if( dtDb.ID == 0 )
                result.DeclaringType = dtDb;
            else result.DeclaringTypeID = dtDb.ID;

            DbContext.LocalTypes.Add( result );

            DbContext.SaveChanges();

            return true;
        }
    }
}