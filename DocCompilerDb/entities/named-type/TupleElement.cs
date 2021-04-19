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
#pragma warning disable 8618

namespace J4JSoftware.DocCompiler
{
    [ EntityConfiguration( typeof(TupleElementConfigurator) ) ]
    public class TupleElement : TypeReference
    {
        public string Name { get; set; }
        public int Index { get; set; }

        public int TupleTypeID { get; set; }
        public TupleType TupleType { get; set; }
    }

    internal class TupleElementConfigurator : EntityConfigurator<TupleElement>
    {
        protected override void Configure( EntityTypeBuilder<TupleElement> builder )
        {
            builder.HasIndex( x => new { x.ID, x.Name } )
                .IsUnique();

            builder.HasIndex( x => new { x.ID, x.Index } )
                .IsUnique();

            builder.HasOne( x => x.TupleType )
                .WithMany( x => x.TupleElements )
                .HasForeignKey( x => x.TupleTypeID )
                .HasPrincipalKey( x => x.ID );
        }
    }
}