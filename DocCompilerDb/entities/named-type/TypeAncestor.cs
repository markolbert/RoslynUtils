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
    [EntityConfiguration(typeof(TypeAncestorConfigurator))]
    public class TypeAncestor
    {
        public int ChildID { get; set; }
        public NamedType ChildType { get; set; }
        public int AncestorID { get; set; }
        public NamedTypeReference AncestorType { get; set; }
    }

    internal class TypeAncestorConfigurator : EntityConfigurator<TypeAncestor>
    {
        protected override void Configure( EntityTypeBuilder<TypeAncestor> builder )
        {
            builder.HasKey( x => new { x.ChildID, x.AncestorID } );

            builder.HasOne( x => x.ChildType )
                .WithMany( x => x.Ancestors )
                .HasForeignKey( x => x.ChildID )
                .HasPrincipalKey( x => x.ID );

            builder.HasOne( x => x.AncestorType )
                .WithMany( x => x.UsedInAncestors )
                .HasForeignKey( x => x.AncestorID )
                .HasPrincipalKey( x => x.ID );
        }
    }
}