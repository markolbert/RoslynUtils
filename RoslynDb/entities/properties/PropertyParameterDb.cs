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
    [ EntityConfiguration( typeof(PropertyParameterConfigurator) ) ]
    public class PropertyParameterDb : ISharpObject
    {
        public int Ordinal { get; set; }

        public int PropertyID { get; set; }
        public PropertyDb Property { get; set; }

        public int ParameterTypeID { get; set; }
        public ImplementableTypeDb ParameterType { get; set; }

        public bool IsAbstract { get; set; }
        public bool IsExtern { get; set; }
        public bool IsOverride { get; set; }
        public bool IsSealed { get; set; }
        public bool IsStatic { get; set; }
        public bool IsVirtual { get; set; }
        public int SharpObjectID { get; set; }
        public SharpObject SharpObject { get; set; }
    }

    internal class PropertyParameterConfigurator : EntityConfigurator<PropertyParameterDb>
    {
        protected override void Configure( EntityTypeBuilder<PropertyParameterDb> builder )
        {
            builder.HasKey( x => x.SharpObjectID );

            builder.HasOne( x => x.Property )
                .WithMany( x => x.Parameters )
                .HasPrincipalKey( x => x.SharpObjectID )
                .HasForeignKey( x => x.PropertyID );

            builder.HasOne( x => x.ParameterType )
                .WithMany( x => x.PropertyParameters )
                .HasPrincipalKey( x => x.SharpObjectID )
                .HasForeignKey( x => x.ParameterTypeID );
        }
    }
}