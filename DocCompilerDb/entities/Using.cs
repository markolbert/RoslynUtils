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

using System.Collections.Generic;
using J4JSoftware.EFCoreUtilities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
#pragma warning disable 8618

namespace J4JSoftware.DocCompiler
{
    [EntityConfiguration(typeof(UsingConfigurator))]
    public class Using : IDeprecation
    {
        public int ID { get; set; }
        public string Name { get;set; }
        public bool Deprecated { get; set; }

        public int AssemblyID { get; set; }
        public Assembly Assembly { get; set; }

        public ICollection<Namespace>? Namespaces { get; set; }
        public ICollection<CodeFile>? CodeFiles {get; set; }
    }

    internal class UsingConfigurator : EntityConfigurator<Using>
    {
        protected override void Configure( EntityTypeBuilder<Using> builder )
        {
            builder.HasIndex( x => x.Name )
                .IsUnique();

            builder.HasMany( x => x.CodeFiles )
                .WithMany( x => x.Usings );

            builder.HasMany( x => x.Namespaces )
                .WithMany( x => x.Usings );

            builder.HasOne( x => x.Assembly )
                .WithMany( x => x.Usings )
                .HasForeignKey( x => x.AssemblyID )
                .HasPrincipalKey( x => x.ID );
        }
    }
}