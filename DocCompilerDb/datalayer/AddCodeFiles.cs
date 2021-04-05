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

using System.Collections.Generic;
using System.Linq;
using J4JSoftware.Logging;

namespace J4JSoftware.DocCompiler
{
    [TopologicalPredecessor(typeof(AddAssemblies))]
    public class AddCodeFiles : EntityProcessor<IScannedFile>
    {
        public AddCodeFiles( 
            IFullyQualifiedNames fqNamers,
            DocDbContext dbContext, 
            IJ4JLogger? logger ) 
            : base( fqNamers, dbContext, logger )
        {
        }

        protected override IEnumerable<IScannedFile> GetNodesToProcess( IDocScanner source ) 
            => source.ScannedFiles;

        protected override bool ProcessEntity( IScannedFile scannedFile )
        {
            var assemblyDb = DbContext.Assemblies
                .FirstOrDefault(x =>
                    x.AssemblyName == scannedFile.BelongsTo.AssemblyName);

            if (assemblyDb == null)
            {
                Logger?.Error<string>("File '{0}' references an Assembly not in the database",
                    scannedFile.SourceFilePath);
                return false;
            }

            var codeFileDb = DbContext.CodeFiles
                .FirstOrDefault(x => x.FullPath == scannedFile.SourceFilePath);

            if (codeFileDb == null)
            {
                codeFileDb = new CodeFile
                {
                    AssemblyID = assemblyDb.ID,
                    FullPath = scannedFile.SourceFilePath
                };

                DbContext.CodeFiles.Add(codeFileDb);
            }
            else
            {
                codeFileDb.Deprecated = false;
                codeFileDb.Assembly = assemblyDb;
            }

            DbContext.SaveChanges();

            return true;
        }
    }
}