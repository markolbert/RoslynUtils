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
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace J4JSoftware.DocCompiler
{
    public partial class DataLayer
    {
        public void UpdateAssembly( IProjectInfo projInfo )
        {
            var assemblyDb = _dbContext.Assemblies
                .FirstOrDefault( x => x.AssemblyName == projInfo.AssemblyName );

            if( assemblyDb == null )
            {
                assemblyDb = new Assembly { AssemblyName = projInfo.AssemblyName };

                _dbContext.Assemblies.Add( assemblyDb );
            }
            else assemblyDb.Deprecated = false;

            assemblyDb.ProjectDirectory = projInfo.ProjectDirectory;
            assemblyDb.Timestamp = DateTime.Now;

            assemblyDb.RootNamespace = projInfo.RootNamespace;
            assemblyDb.TargetFrameworks = projInfo.TargetFrameworks;

            assemblyDb.Authors = projInfo.Authors;
            assemblyDb.Company = projInfo.Company;
            assemblyDb.Copyright = projInfo.Copyright;
            assemblyDb.Description = projInfo.Description;

            assemblyDb.PackageDescription = projInfo.PackageDescription;
            assemblyDb.PackageLicense = projInfo.PackageLicense;

            assemblyDb.RepositoryUrl = projInfo.RepositoryUrl;
            assemblyDb.RepositoryType = projInfo.RepositoryType;

            assemblyDb.AssemblyVersion = projInfo.AssemblyVersion;
            assemblyDb.FileVersion = projInfo.FileVersion;
            assemblyDb.Version = projInfo.Version;

            _dbContext.SaveChanges();
        }
    }
}