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
using System.Collections.Generic;
using System.ComponentModel;
using J4JSoftware.EFCoreUtilities;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
#pragma warning disable 8618

namespace J4JSoftware.DocCompiler
{
    [EntityConfiguration(typeof(AssemblyConfigurator))]
    public class Assembly : IDeprecation
    {
        public int ID { get; set; }
        public bool Deprecated { get; set; }
        public DateTime Timestamp { get; set; }

        public string ProjectDirectory { get; set; }
        public string AssemblyName { get; set; }
        public string RootNamespace { get; set; }
        public string TargetFrameworks { get; set; }
        public NullableContextOptions NullableContextOptions { get; set; }

        public string Authors { get; set; }
        public string Company { get; set; }
        public string Description { get; set; }
        public string Copyright { get; set; }

        public string PackageDescription { get;set; }
        public string PackageLicense { get; set; }
        public string RepositoryUrl { get; set; }
        public string RepositoryType { get; set; }
        
        public string Version { get; set; }
        public string AssemblyVersion { get; set; }
        public string FileVersion { get; set; }
        
        public ICollection<Namespace> Namespaces { get; set; }
        public ICollection<CodeFile> CodeFiles { get; set; }
        public ICollection<Using> Usings { get; set; }
        public Documentation Documentation { get; set; }
    }

    internal class AssemblyConfigurator : EntityConfigurator<Assembly>
    {
        protected override void Configure( EntityTypeBuilder<Assembly> builder )
        {
            builder.Property( x => x.NullableContextOptions )
                .HasConversion<string>();

            builder.Property( x => x.Timestamp )
                .HasDefaultValue( DateTime.Now );
        }
    }
}