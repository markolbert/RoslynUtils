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
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
#pragma warning disable 8618

namespace J4JSoftware.DocCompiler
{
    [EntityConfiguration(typeof(TypeParameterConfigurator))]
    public class TypeParameter
    {
        public int ID { get; set; }
        public int DefinedInID { get; set; }
        public DocumentedType DefinedIn { get; set; }

        public int Index { get; set; }
        public string Name { get; set; }

        public ICollection<TypeConstraint> TypeConstraints { get; set; }
        public GeneralTypeConstraints GeneralTypeConstraints { get; set; }
    }

    internal class TypeParameterConfigurator : EntityConfigurator<TypeParameter>
    {
        protected override void Configure( EntityTypeBuilder<TypeParameter> builder )
        {
            builder.HasIndex( x => new { x.DefinedInID, x.Name } )
                .IsUnique();

            builder.HasOne( x => x.DefinedIn )
                .WithMany( x => x.TypeParameters )
                .HasForeignKey( x => x.DefinedInID )
                .HasPrincipalKey( x => x.ID );

            builder.Property( x => x.GeneralTypeConstraints )
                .HasDefaultValue( GeneralTypeConstraints.None )
                .HasConversion<string>();
        }
    }
}