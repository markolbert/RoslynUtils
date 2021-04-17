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
    [TopologicalPredecessor(typeof(LocalTypeResolver))]
    public class ExternalTypeResolver : TypeResolver<ExternalType>
    {
        public ExternalTypeResolver( 
            DocDbContext dbContext, 
            IJ4JLogger? logger 
        ) 
            : base( dbContext, logger )
        {
        }

        protected override bool FindTypeInternal( TypeReferenceInfo typeInfo, NamedType ntDb, out ExternalType? result )
        {
            result = null;

            var possibleNamedTypes = DbContext.ExternalTypes
                .Include( x => x.PossibleNamespaces )
                .Where( x => x.Name == typeInfo.Name 
                             && x.NumTypeParameters == typeInfo.Arguments.Count );

            if( !possibleNamedTypes.Any() )
            {
                result = CreateIfMissing ? CreateExternalType( typeInfo ) : null;

                return result != null;
            }

            foreach( var extTypeDb in possibleNamedTypes )
            {
                if( !( extTypeDb.PossibleNamespaces?
                    .Any( x => NamespaceContexts!.Any( y => y.NamespaceName == x.Name ) ) ?? false ) ) 
                    continue;

                result = extTypeDb;

                return true;
            }

            // if we get here there were no matches among the ExternalTypes in the database
            result = CreateIfMissing ? CreateExternalType( typeInfo ) : null;

            return result != null;
        }

        private ExternalType CreateExternalType( TypeReferenceInfo typeInfo )
        {
            var retVal = new ExternalType
            {
                Name = typeInfo.Name,
                Accessibility = Accessibility.ExternallyDefined,
                NumTypeParameters = typeInfo.Arguments.Count,
                PossibleNamespaces = NamespaceContexts!.Select( x => x.Namespace ).ToList()
            };

            DbContext.ExternalTypes.Add( retVal );

            return retVal;
        }
    }
}