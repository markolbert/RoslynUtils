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
    [TopologicalRoot]
    public class DocumentedTypeResolver : TypeResolver<DocumentedType>
    {
        public DocumentedTypeResolver( 
            DocDbContext dbContext, 
            IJ4JLogger? logger ) 
            : base( dbContext, logger )
        {
        }

        protected override bool FindTypeInternal( TypeReferenceInfo typeInfo, NamedType ntDb, out DocumentedType? result )
        {
            result = null;

            foreach( var nsContext in NamespaceContexts! )
            {
                result = DbContext.DocumentedTypes
                    .Include( x => x.TypeParameters )
                    .FirstOrDefault( x =>
                        x.FullyQualifiedNameWithoutTypeParameters == $"{nsContext.NamespaceName}.{typeInfo.Name}"
                        && ( x.TypeParameters == null && !typeInfo.Arguments.Any()
                             || x.TypeParameters != null && x.TypeParameters.Count == typeInfo.Arguments.Count
                        ) );

                if( result != null )
                    return true;
            }

            return false;
        }
    }
}