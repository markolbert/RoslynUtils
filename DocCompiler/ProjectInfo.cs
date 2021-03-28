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
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;
#pragma warning disable 8618

namespace J4JSoftware.DocCompiler
{
    public class ProjectInfo
    {
        public static string[] ExcludedProjectDirectories = new[] { "bin", "obj" };

        private readonly List<string> _excludedFiles;
        private readonly List<string> _sourceFiles;

        internal ProjectInfo(
            string projDir,
            XDocument projDoc,
            XElement projElem,
            StringComparison osFileComparison
        )
        {
            ProjectDirectory = projDir;
            ProjectDocument = projDoc;
            ProjectElement = projElem;

            AssemblyName = ProjectElement.Descendants( "AssemblyName" ).FirstOrDefault()?.Value ?? string.Empty;
            RootNamespace = ProjectElement.Descendants( "RootNamespace" ).FirstOrDefault()?.Value ?? string.Empty;
            Authors = ProjectElement.Descendants( "Authors" ).FirstOrDefault()?.Value ?? string.Empty;
            Company = ProjectElement.Descendants( "Company" ).FirstOrDefault()?.Value ?? string.Empty;
            Description = ProjectElement.Descendants( "Description" ).FirstOrDefault()?.Value ?? string.Empty;
            Copyright = ProjectElement.Descendants( "Copyright" ).FirstOrDefault()?.Value ?? string.Empty;

            var nullableText = ProjectElement.Descendants( "Nullable" ).FirstOrDefault()?.Value;

            if( Enum.TryParse( typeof(NullableContextOptions),
                nullableText,
                true,
                out var ncTemp ) )
                NullableContextOptions = (NullableContextOptions) ncTemp!;
            else NullableContextOptions = NullableContextOptions.Disable;

            var excludedDirs = ExcludedProjectDirectories.Select( x => Path.Combine( ProjectDirectory, x ) )
                .ToList();

            _excludedFiles = ProjectDocument.Root!
                .Descendants()
                .Where( e => e.Name == "Compile" && e.Attribute( "Remove" ) != null )
                .Select( e => e.Attribute( "Remove" )?.Value! )
                .ToList();

            _sourceFiles = Directory
                .GetFiles( ProjectDirectory, "*.cs", SearchOption.AllDirectories )
                .Where( f =>
                    !excludedDirs.Any( x => x.Equals( Path.GetDirectoryName( f ), osFileComparison ) )
                    && !ExcludedFiles.Any( x => x.Equals( f, osFileComparison ) ) )
                .ToList();
        }

        public string ProjectDirectory { get; }
        public XDocument ProjectDocument { get; }
        public XElement ProjectElement { get; }
        public NullableContextOptions NullableContextOptions { get; }

        public string AssemblyName { get; }
        public string RootNamespace { get; }
        public string Authors { get; }
        public string Company { get; }
        public string Description { get; }
        public string Copyright { get; }

        public ReadOnlyCollection<string> ExcludedFiles => _excludedFiles.AsReadOnly();
        public ReadOnlyCollection<string> SourceCodeFiles => _sourceFiles.AsReadOnly();
    }
}