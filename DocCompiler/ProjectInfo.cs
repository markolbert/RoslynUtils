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
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using J4JSoftware.EFCoreUtilities;
using Microsoft.CodeAnalysis;
#pragma warning disable 8618

namespace J4JSoftware.DocCompiler
{
    public class ProjectInfo : IProjectInfo
    {
        public static string[] ExcludedProjectDirectories = new[] { "bin", "obj" };

        private readonly List<string> _excludedFiles;
        private readonly List<string> _sourceFiles;

        internal ProjectInfo(
            string projDir,
            XDocument projDoc,
            XElement projElem
        )
        {
            ProjectDirectory = projDir;
            ProjectDocument = projDoc;
            ProjectElement = projElem;

            AssemblyName = ProjectElement.Descendants( "AssemblyName" ).FirstOrDefault()?.Value ?? string.Empty;
            RootNamespace = ProjectElement.Descendants( "RootNamespace" ).FirstOrDefault()?.Value ?? string.Empty;

            var nullableText = ProjectElement.Descendants( "Nullable" ).FirstOrDefault()?.Value;

            if( Enum.TryParse( typeof(NullableContextOptions),
                nullableText,
                true,
                out var ncTemp ) )
                NullableContextOptions = (NullableContextOptions) ncTemp!;
            else NullableContextOptions = NullableContextOptions.Disable;

            TargetFrameworks = ProjectElement.Descendants( "TargetFramework" ).FirstOrDefault()?.Value ?? string.Empty;
            if( string.IsNullOrEmpty(TargetFrameworks  ))
                TargetFrameworks = ProjectElement.Descendants( "TargetFrameworks" ).FirstOrDefault()?.Value ?? string.Empty;
            
            Authors = ProjectElement.Descendants( "Authors" ).FirstOrDefault()?.Value ?? string.Empty;
            Company = ProjectElement.Descendants( "Company" ).FirstOrDefault()?.Value ?? string.Empty;
            Description = ProjectElement.Descendants( "Description" ).FirstOrDefault()?.Value ?? string.Empty;
            Copyright = ProjectElement.Descendants( "Copyright" ).FirstOrDefault()?.Value ?? string.Empty;

            PackageDescription = ProjectElement.Descendants( "PackageDescription" ).FirstOrDefault()?.Value ?? string.Empty;
            PackageLicense = ProjectElement.Descendants( "PackageLicenseExpression" ).FirstOrDefault()?.Value ??
                             string.Empty;
            RepositoryUrl = ProjectElement.Descendants( "RepositoryUrl" ).FirstOrDefault()?.Value ?? string.Empty;
            RepositoryType = ProjectElement.Descendants( "RepositoryType" ).FirstOrDefault()?.Value ?? string.Empty;
            Version = ProjectElement.Descendants( "Version" ).FirstOrDefault()?.Value ?? string.Empty;
            AssemblyVersion = ProjectElement.Descendants( "AssemblyVersion" ).FirstOrDefault()?.Value ?? string.Empty;
            FileVersion = ProjectElement.Descendants( "FileVersion" ).FirstOrDefault()?.Value ?? string.Empty;
            
            var excludedDirs = ExcludedProjectDirectories.Select( x => Path.Combine( ProjectDirectory, x ) )
                .ToList();

            _excludedFiles = ProjectDocument.Root!
                .Descendants()
                .Where( e => e.Name == "Compile" && e.Attribute( "Remove" ) != null )
                .Select( e => e.Attribute( "Remove" )?.Value! )
                .ToList();

            var objDir = Path.Combine( ProjectDirectory, "obj" );

            _sourceFiles = Directory
                .GetFiles( ProjectDirectory, "*.cs", SearchOption.AllDirectories )
                .Where( f =>
                    f.IndexOf( objDir, OsUtilities.FileSystemComparison ) != 0
                    && !excludedDirs.Any(
                        x => x.Equals( Path.GetDirectoryName( f ), OsUtilities.FileSystemComparison ) )
                    && !ExcludedFiles.Any( x => x.Equals( f, OsUtilities.FileSystemComparison ) ) )
                .ToList();
        }

        public string ProjectDirectory { get; }
        public XDocument ProjectDocument { get; }
        public XElement ProjectElement { get; }

        public string AssemblyName { get; }
        public string RootNamespace { get; }
        public NullableContextOptions NullableContextOptions { get; }
        public string TargetFrameworks { get; }

        public string Authors { get; }
        public string Company { get; }
        public string Description { get; }
        public string Copyright { get; }

        public string PackageDescription { get; }
        public string PackageLicense { get; }
        public string RepositoryUrl { get; }
        public string RepositoryType { get; }
        
        public string Version { get; }
        public string AssemblyVersion { get; }
        public string FileVersion { get; }

        public ReadOnlyCollection<string> ExcludedFiles => _excludedFiles.AsReadOnly();
        public ReadOnlyCollection<string> SourceCodeFiles => _sourceFiles.AsReadOnly();
    }
}