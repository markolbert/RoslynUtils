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
#pragma warning disable 8602
#pragma warning disable 8618

namespace J4JSoftware.DocCompiler
{
    [ EntityConfiguration( typeof(TypeReferenceConfigurator) ) ]
    public class TypeReference
    {
        public int ID { get; set; }
     
        public int ReferencedTypeID { get; set; }
        public NamedType ReferencedType { get; set; }
        
        public int ReferencedTypeRank { get; set; }

        public int? ParentReferenceID { get; set; }
        public TypeReference? ParentReference { get; set; }
        public ICollection<TypeReference> ChildReferences { get; set; }

        public ICollection<TypeArgument> UsedInTypeArguments { get; set; }
        public ICollection<TypeAncestor> UsedInAncestors { get; set; }
        public ICollection<TypeConstraint> UsedInConstraints { get; set; }
        public ICollection<Method> MethodReturnTypes { get; set; }
    }

    internal class TypeReferenceConfigurator : EntityConfigurator<TypeReference>
    {
        protected override void Configure( EntityTypeBuilder<TypeReference> builder )
        {
            builder.HasOne( x => x.ReferencedType )
                .WithMany( x => x.UsedInReferences )
                .HasForeignKey( x => x.ReferencedTypeID )
                .HasPrincipalKey( x => x.ID );

            builder.HasOne( x => x.ParentReference )
                .WithMany( x => x.ChildReferences )
                .HasForeignKey( x => x.ParentReferenceID )
                .HasPrincipalKey( x => x.ID );
        }
    }
}