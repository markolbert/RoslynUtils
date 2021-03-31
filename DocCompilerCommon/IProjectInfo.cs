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

using System.Collections.ObjectModel;
using System.Xml.Linq;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.DocCompiler
{
    public interface IProjectInfo
    {
        string ProjectDirectory { get; }
        string AssemblyName { get; }
        string RootNamespace { get; }
        NullableContextOptions NullableContextOptions { get; }
        string TargetFrameworks { get; }
        string Authors { get; }
        string Company { get; }
        string Description { get; }
        string Copyright { get; }
        string PackageDescription { get; }
        string PackageLicense { get; }
        string RepositoryUrl { get; }
        string RepositoryType { get; }
        string Version { get; }
        string AssemblyVersion { get; }
        string FileVersion { get; }
        ReadOnlyCollection<string> SourceCodeFiles { get; }
    }
}