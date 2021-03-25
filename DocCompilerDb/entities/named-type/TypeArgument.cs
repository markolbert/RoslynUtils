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
    [EntityConfiguration(typeof(TypeArgumentConfigurator))]
    public class TypeArgument
    {
        public int ReferencedTypeID { get; set; }
        public TypeReference ReferencedType { get; set; }
        public int DeclaringTypeID { get; set; }
        public NamedType DeclaringType { get; set; }
        public int Index { get; set; }
    }

    internal class TypeArgumentConfigurator : EntityConfigurator<TypeArgument>
    {
        protected override void Configure( EntityTypeBuilder<TypeArgument> builder )
        {
            builder.HasKey( x => new { x.DeclaringTypeID, x.Index } );

            builder.HasOne( x => x.DeclaringType )
                .WithMany( x => x.TypeArguments )
                .HasForeignKey( x => x.DeclaringTypeID )
                .HasPrincipalKey( x => x.ID );

            builder.HasOne( x => x.ReferencedType )
                .WithMany( x => x.UsedInTypeArguments )
                .HasForeignKey( x => x.ReferencedTypeID )
                .HasPrincipalKey( x => x.ID );
        }
    }
}