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

using System;
using J4JSoftware.EFCoreUtilities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

#pragma warning disable 8602

namespace J4JSoftware.Roslyn
{
    [ EntityConfiguration( typeof(ParametricTypeDbConfigurator) ) ]
    public class ParametricTypeDb : BaseTypeDb, IParametricTypeEntity
    {
        public int? ContainingTypeID { get; set; }
        public BaseTypeDb? ContainingType { get; set; }
        public ParametricTypeConstraint Constraints { get; set; }

        int? IParametricTypeEntity.ContainerID
        {
            get => ContainingTypeID;
            set => ContainingTypeID = value;
        }

        object? IParametricTypeEntity.Container
        {
            get => ContainingType;

            set
            {
                if( value is BaseTypeDb typeDb )
                    ContainingType = typeDb;
                else throw new InvalidCastException( $"Expected a {typeof(BaseTypeDb)} but got a {value.GetType()}" );
            }
        }
    }

    internal class ParametricTypeDbConfigurator : EntityConfigurator<ParametricTypeDb>
    {
        protected override void Configure( EntityTypeBuilder<ParametricTypeDb> builder )
        {
            builder.HasOne( x => x.ContainingType )
                .WithMany( x => x.ParametricTypes )
                .HasPrincipalKey( x => x.SharpObjectID )
                .HasForeignKey( x => x.ContainingTypeID );

            builder.Property( x => x.Constraints )
                .HasConversion<string>();
        }
    }
}