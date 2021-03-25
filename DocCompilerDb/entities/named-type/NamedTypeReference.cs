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

using System.Collections;
using System.Collections.Generic;
using J4JSoftware.EFCoreUtilities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace J4JSoftware.DocCompiler
{
    [EntityConfiguration(typeof(NamedTypeReferenceConfigurator))]
    public class NamedTypeReference
    {
        public int ID { get; set; }
        public string Name { get; set; }

        public int NamespaceID { get; set; }
        public Namespace Namespace { get; set; }

        public string? ExternalUrl { get; set; }
        public bool IsExternal => !string.IsNullOrEmpty( ExternalUrl );

        public ICollection<TypeConstraint> UsedInConstraints { get; set; }
        public ICollection<TypeReference> UsedInReferences { get;set; }
        public ICollection<TypeAncestor> UsedInAncestors { get; set; }
        public ICollection<Event> UsedInEvents { get;set; }
        public ICollection<Property> PropertyReturnTypes { get; set; }
        public ICollection<Method> MethodReturnTypes { get; set; }
        public ICollection<Argument> UsedInArguments { get; set; }
        public ICollection<Field> FieldTypes { get; set; }
    }

    internal class NamedTypeReferenceConfigurator : EntityConfigurator<NamedTypeReference>
    {
        protected override void Configure( EntityTypeBuilder<NamedTypeReference> builder )
        {
            builder.HasIndex( x => x.Name )
                .IsUnique();

            builder.HasOne( x => x.Namespace )
                .WithMany( x => x.NamedTypeReferences )
                .HasForeignKey( x => x.NamespaceID )
                .HasPrincipalKey( x => x.ID );
        }
    }
}