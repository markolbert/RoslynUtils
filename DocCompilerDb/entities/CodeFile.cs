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
using J4JSoftware.EFCoreUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace J4JSoftware.DocCompiler
{
    [EntityConfiguration(typeof(CodeFileConfigurator))]
    public class CodeFile : IDeprecation
    {
        public int ID { get; set; }
        public bool Deprecated { get; set; }
        public string FullPath { get; set; }
        public int AssemblyID { get; set; }
        public Assembly Assembly { get; set; }
        public ICollection<NamedType> NamedTypes { get; set; }
    }

    internal class CodeFileConfigurator : EntityConfigurator<CodeFile>
    {
        protected override void Configure( EntityTypeBuilder<CodeFile> builder )
        {
            builder.HasIndex( x => x.FullPath )
                .IsUnique();

            builder.HasOne( x => x.Assembly )
                .WithMany( x => x.CodeFiles )
                .HasForeignKey( x => x.AssemblyID )
                .HasPrincipalKey( x => x.ID );

            builder.Property( x => x.FullPath )
                .UseOsCollation();
        }
    }
}