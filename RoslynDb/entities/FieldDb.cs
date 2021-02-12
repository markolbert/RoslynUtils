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
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

#pragma warning disable 8618
#pragma warning disable 8603
#pragma warning disable 8602

namespace J4JSoftware.Roslyn
{
    [ EntityConfiguration( typeof(FieldConfigurator) ) ]
    public class FieldDb : ISharpObject
    {
        public Accessibility Accessibility { get; set; }
        public DeclarationModifier DeclarationModifier { get; set; }

        public bool IsAbstract { get; set; }
        public bool IsExtern { get; set; }
        public bool IsOverride { get; set; }
        public bool IsReadOnly { get; set; }
        public bool IsSealed { get; set; }
        public bool IsStatic { get; set; }
        public bool IsVirtual { get; set; }

        public int DefiningTypeID { get; set; }
        public ImplementableTypeDb? DefiningType { get; set; } = null!;

        public int FieldTypeID { get; set; }
        public BaseTypeDb FieldType { get; set; } = null!;
        public int SharpObjectID { get; set; }
        public SharpObject SharpObject { get; set; }
    }

    internal class FieldConfigurator : EntityConfigurator<FieldDb>
    {
        protected override void Configure( EntityTypeBuilder<FieldDb> builder )
        {
            builder.HasKey( x => x.SharpObjectID );

            builder.HasOne( x => x.SharpObject )
                .WithOne( x => x.Field )
                .HasPrincipalKey<SharpObject>( x => x.ID )
                .HasForeignKey<FieldDb>( x => x.SharpObjectID );

            builder.HasOne( x => x.DefiningType )
                .WithMany( x => x.Fields )
                .HasForeignKey( x => x.DefiningTypeID )
                .HasPrincipalKey( x => x.SharpObjectID );

            builder.HasOne( x => x.FieldType )
                .WithMany( x => x.FieldTypes )
                .HasPrincipalKey( x => x.SharpObjectID )
                .HasForeignKey( x => x.FieldTypeID );

            builder.Property( x => x.Accessibility )
                .HasConversion<string>();

            builder.Property( x => x.DeclarationModifier )
                .HasConversion<string>();
        }
    }
}