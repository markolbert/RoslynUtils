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
using System.Collections.ObjectModel;
using System.Linq;
using System.Xml.Linq;
using Microsoft.CodeAnalysis;
#pragma warning disable 8618

namespace J4JSoftware.DocCompiler
{
    public class ParsedProject
    {
        private XElement _projElement = new( "invalid" );
        private List<string> _excludedFiles = new();
        private List<string> _sourceFiles = new();
        private List<ParsedSourceFile> _parsedFiles = new();

        public XDocument ProjectDocument { get; internal set; } = new();

        public XElement ProjectElement
        {
            get => _projElement;

            internal set
            {
                _projElement = value;

                AssemblyName = _projElement.Descendants( "AssemblyName" ).FirstOrDefault()?.Value ?? string.Empty;
                RootNamespace = _projElement.Descendants( "RootNamespace" ).FirstOrDefault()?.Value ?? string.Empty;
                Authors = _projElement.Descendants( "Authors" ).FirstOrDefault()?.Value ?? string.Empty;
                Company = _projElement.Descendants( "Company" ).FirstOrDefault()?.Value ?? string.Empty;
                Description = _projElement.Descendants( "Description" ).FirstOrDefault()?.Value ?? string.Empty;
                Copyright = _projElement.Descendants( "Copyright" ).FirstOrDefault()?.Value ?? string.Empty;
            }
        }

        public NullableContextOptions NullableContextOptions { get; internal set; } = NullableContextOptions.Disable;

        public string AssemblyName { get; private set; } = string.Empty;
        public string RootNamespace { get; private set; } = string.Empty;
        public string Authors { get; private set; } = string.Empty;
        public string Company { get; private set; } = string.Empty;
        public string Description { get; private set; } = string.Empty;
        public string Copyright { get; private set; } = string.Empty;

        public string ProjectDirectory { get; internal set; } = string.Empty;
        public ReadOnlyCollection<string> ExcludedFiles => _excludedFiles.AsReadOnly();
        internal void AddExcludedFiles( IEnumerable<string> toExclude ) => _excludedFiles.AddRange( toExclude );
        public ReadOnlyCollection<string> SourceCodeFiles => _sourceFiles.AsReadOnly();
        internal void AddSourceCodeFiles( IEnumerable<string> toAdd ) => _sourceFiles.AddRange( toAdd );

        public bool IsParsed => ParsedSourceFiles.Count > 0 && _parsedFiles.TrueForAll( x => x.IsParsed );
        public ReadOnlyCollection<ParsedSourceFile> ParsedSourceFiles => _parsedFiles.AsReadOnly();
        internal void AddParsedFiles( IEnumerable<ParsedSourceFile> toAdd ) => _parsedFiles.AddRange( toAdd );
    }
}