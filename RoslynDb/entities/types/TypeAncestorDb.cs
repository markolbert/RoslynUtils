#region license

// Copyright 2021 Mark A. Olbert
// 
// This library or program 'RoslynDb' is free software: you can redistribute it
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

#pragma warning disable 8618

namespace J4JSoftware.Roslyn
{
    [ EntityConfiguration( typeof(TypeImplementationConfigurator) ) ]
    public class TypeAncestorDb : ISynchronized
    {
        public int ChildTypeID { get; set; }
        public BaseTypeDb ChildType { get; set; }
        public int AncestorTypeID { get; set; }
        public BaseTypeDb AncestorType { get; set; }
        public bool Synchronized { get; set; }
    }

    internal class TypeImplementationConfigurator : EntityConfigurator<TypeAncestorDb>
    {
        protected override void Configure( EntityTypeBuilder<TypeAncestorDb> builder )
        {
            builder.HasOne( x => x.AncestorType )
                .WithMany( x => x.AncestorTypes )
                .HasForeignKey( x => x.AncestorTypeID )
                .HasPrincipalKey( x => x.SharpObjectID );

            builder.HasKey( x => new { x.ChildTypeID, x.AncestorTypeID } );
        }
    }
}