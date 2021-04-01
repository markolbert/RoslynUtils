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

namespace J4JSoftware.DocCompiler
{
    public partial class DataLayer
    {
        public bool UpdateCodeFile( IScannedFile scannedFile )
        {
            var assemblyDb = _dbContext.Assemblies
                .FirstOrDefault( x =>
                    x.AssemblyName == scannedFile.BelongsTo.AssemblyName );

            if( assemblyDb == null )
            {
                _logger?.Error<string>( "File '{0}' references an Assembly not in the database",
                    scannedFile.SourceFilePath );
                return false;
            }

            var codeFileDb = _dbContext.CodeFiles
                .FirstOrDefault( x => x.FullPath == scannedFile.SourceFilePath );

            if( codeFileDb == null )
            {
                codeFileDb = new CodeFile
                {
                    AssemblyID = assemblyDb.ID,
                    FullPath = scannedFile.SourceFilePath
                };

                _dbContext.CodeFiles.Add( codeFileDb );
            }
            else
            {
                codeFileDb.Deprecated = false;
                codeFileDb.Assembly = assemblyDb;
            }

            _dbContext.SaveChanges();

            return true;
        }
    }
}