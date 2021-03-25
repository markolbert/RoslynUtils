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

using J4JSoftware.EFCoreUtilities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace J4JSoftware.DocCompiler
{
    [EntityConfiguration(typeof(TypeConstraintConfigurator))]
    public class TypeConstraint
    {
        public int ConstrainedTypeParameterID { get; set; }
        public TypeParameter ConstrainedTypeParameter { get; set; }

        public int ConstraintID { get; set; }
        public NamedTypeReference Constraint { get; set; }
    }

    internal class TypeConstraintConfigurator : EntityConfigurator<TypeConstraint>
    {
        protected override void Configure( EntityTypeBuilder<TypeConstraint> builder )
        {
            builder.HasKey( x => new { x.ConstrainedTypeParameterID, x.ConstraintID } );

            builder.HasOne( x => x.Constraint )
                .WithMany( x => x.UsedInConstraints )
                .HasForeignKey( x => x.ConstraintID )
                .HasPrincipalKey( x => x.ID );

            builder.HasOne( x => x.ConstrainedTypeParameter )
                .WithMany( x => x.TypeConstraints )
                .HasForeignKey( x => x.ConstrainedTypeParameterID )
                .HasPrincipalKey( x => x.ID );
        }
    }
}