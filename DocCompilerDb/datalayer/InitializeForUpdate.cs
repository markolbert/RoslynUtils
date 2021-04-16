#region license

// Copyright 2021 Mark A. Olbert
// 
// This library or program 'DocCompiler' is free software: you can redistribute it
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
    [ TopologicalRoot() ]
    public class InitializeForUpdate : EntityProcessor<IProjectInfo>
    {
        public static readonly string[] DocumentedTables =
            { "Assemblies", "Namespaces", "NamedTypes", "Arguments", "Methods", "Properties", "Events", "Fields" };

        public static readonly string[] RelationshipTables =
            { "TypeAncestors", "TypeConstraints", "TypeParameters", "TypeArguments" };

        public static readonly string[] AutoIncrementTables =
            { "TypeParameters", "TypeReferences"  };

        public InitializeForUpdate(
            IFullyQualifiedNames fqNamers,
            DocDbContext dbContext,
            IJ4JLogger? logger )
            : base( fqNamers, dbContext, logger )
        {
        }

        protected override IEnumerable<IProjectInfo> GetNodesToProcess( IDocScanner source ) => source.Projects;

        // mark all documentable items as deprecated so we can tell which items are
        // newly added, and delete all entries in relationship/linking tables
        protected override bool ProcessEntity( IProjectInfo projInfo )
            => DoBulkProcessing( DocumentedTables, "update {0} set Deprecated = 0", "deprecating" )
               && DoBulkProcessing( RelationshipTables, "delete from {0}", "truncating" )
               && DoBulkProcessing( AutoIncrementTables,
                   "delete from sqlite_sequence where name = '{0}'",
                   "resetting autoincrement index" )
               && ClearTypeReferences();

        private bool DoBulkProcessing( string[] tables, string cmdText, string text )
        {
            string? curTable = null;

            try
            {
                foreach( var docTable in tables )
                {
                    curTable = docTable;
                    var cmd = string.Format( cmdText, docTable );

                    DbContext.Database.ExecuteSqlRaw( cmd );
                }

                DbContext.SaveChanges();
            }
            catch( Exception e )
            {
                Logger?.Error(
                    $"Exception thrown while {text} {curTable!}. Message was '{e.Message}'" );

                return false;
            }

            return true;
        }

        private bool ClearTypeReferences()
        {
            // we have to do this in three steps because the table is self-referential
            try
            {
                DbContext.Database.ExecuteSqlRaw( "update TypeReferences set ParentReferenceID = null" );
                DbContext.Database.ExecuteSqlRaw( "delete from TypeReferences" );
                DbContext.Database.ExecuteSqlRaw( "delete from sqlite_sequence where name = 'TypeReferences'" );

                DbContext.SaveChanges();
            }
            catch( Exception e )
            {
                Logger?.Error(
                    $"Exception thrown while clearing TypeReferences. Message was '{e.Message}'" );

                return false;
            }

            return true;
        }
    }
}