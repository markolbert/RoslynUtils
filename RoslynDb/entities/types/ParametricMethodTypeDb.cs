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
    [ EntityConfiguration( typeof(ParametricMethodTypeDbConfigurator) ) ]
    public class ParametricMethodTypeDb : BaseTypeDb, IParametricTypeEntity
    {
        public int? ContainingMethodID { get; set; }
        public MethodDb? ContainingMethod { get; set; }
        public ParametricTypeConstraint Constraints { get; set; }

        int? IParametricTypeEntity.ContainerID
        {
            get => ContainingMethodID;
            set => ContainingMethodID = value;
        }

        object? IParametricTypeEntity.Container
        {
            get => ContainingMethod;

            set
            {
                if( value is MethodDb methodDb )
                    ContainingMethod = methodDb;
                else throw new InvalidCastException( $"Expected a {typeof(MethodDb)} but got a {value.GetType()}" );
            }
        }
    }

    internal class ParametricMethodTypeDbConfigurator : EntityConfigurator<ParametricMethodTypeDb>
    {
        protected override void Configure( EntityTypeBuilder<ParametricMethodTypeDb> builder )
        {
            builder.HasOne( x => x.ContainingMethod )
                .WithMany( x => x.ParametricTypes )
                .HasPrincipalKey( x => x.SharpObjectID )
                .HasForeignKey( x => x.ContainingMethodID );

            builder.Property( x => x.Constraints )
                .HasConversion<string>();
        }
    }
}