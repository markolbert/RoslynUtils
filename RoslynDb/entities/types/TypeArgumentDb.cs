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

#pragma warning disable 8602

namespace J4JSoftware.Roslyn
{
    [ EntityConfiguration( typeof(TypeArgumentConfigurator) ) ]
    public class TypeArgumentDb : ISynchronized
    {
        public int ID { get; set; }

        public int DeclaringTypeID { get; set; }
        public GenericTypeDb? DeclaringType { get; set; }

        public int Ordinal { get; set; }
        public int ArgumentTypeID { get; set; }
        public BaseTypeDb? ArgumentType { get; set; }
        public bool Synchronized { get; set; }
    }

    internal class TypeArgumentConfigurator : EntityConfigurator<TypeArgumentDb>
    {
        protected override void Configure( EntityTypeBuilder<TypeArgumentDb> builder )
        {
            builder.HasOne( x => x.ArgumentType )
                .WithMany( x => x.TypeArgumentReferences )
                .HasPrincipalKey( x => x.SharpObjectID )
                .HasForeignKey( x => x.ArgumentTypeID );

            builder.HasOne( x => x.DeclaringType )
                .WithMany( x => x.TypeArguments )
                .HasPrincipalKey( x => x.SharpObjectID )
                .HasForeignKey( x => x.DeclaringTypeID );

            builder.HasIndex( x => new { TypeDefinitionID = x.DeclaringTypeID, x.Ordinal } )
                .IsUnique();
        }
    }
}