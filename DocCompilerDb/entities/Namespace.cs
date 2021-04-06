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
using System.Linq;
using J4JSoftware.EFCoreUtilities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
#pragma warning disable 8602
#pragma warning disable 8618

namespace J4JSoftware.DocCompiler
{
    [EntityConfiguration(typeof(NamespaceConfigurator))]
    public class Namespace : IDeprecation
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string FullyQualifiedName { get; set; }
        public bool Deprecated { get; set; }
        public ICollection<Assembly> Assemblies { get; set; }
        public ICollection<DocumentedType> DocumentedTypes { get; set; }

        public Documentation Documentation { get; set; }

        public int? ContainingNamespaceID { get; set; }
        public Namespace? ContainingNamespace { get; set; }
        public ICollection<Namespace> ChildNamespaces { get; set; }

        public ICollection<CodeFile> CodeFiles { get; set; }
        public ICollection<Using> Usings { get; set; }
    }

    internal class NamespaceConfigurator : EntityConfigurator<Namespace>
    {
        protected override void Configure( EntityTypeBuilder<Namespace> builder )
        {
            builder.HasIndex( x => x.FullyQualifiedName )
                .IsUnique();

            builder.HasMany( x => x.Assemblies )
                .WithMany( x => x.Namespaces );

            builder.HasMany( x => x.CodeFiles )
                .WithMany( x => x.Namespaces );

            builder.HasOne(x => x.ContainingNamespace)
                .WithMany(x => x.ChildNamespaces)
                .HasForeignKey(x => x.ContainingNamespaceID)
                .HasPrincipalKey(x => x.ID);
        }
    }
}