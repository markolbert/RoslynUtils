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

using System.Collections.Generic;
using J4JSoftware.EFCoreUtilities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

#pragma warning disable 8618

namespace J4JSoftware.Roslyn
{
    [ EntityConfiguration( typeof(AttributeConfigurator) ) ]
    public class AttributeDb
    {
        public int ID { get; set; }
        public int TargetObjectID { get; set; }
        public SharpObject TargetObject { get; set; }
        public int AttributeTypeID { get; set; }
        public ImplementableTypeDb AttributeType { get; set; }

        public List<AttributeArgumentDb> Arguments { get; set; }
    }

    internal class AttributeConfigurator : EntityConfigurator<AttributeDb>
    {
        protected override void Configure( EntityTypeBuilder<AttributeDb> builder )
        {
            builder.HasOne( x => x.TargetObject )
                .WithMany( x => x.Attributes )
                .HasPrincipalKey( x => x.ID )
                .HasForeignKey( x => x.TargetObjectID );

            builder.HasOne( x => x.AttributeType )
                .WithMany( x => x.AttributeReferences )
                .HasPrincipalKey( x => x.SharpObjectID )
                .HasForeignKey( x => x.AttributeTypeID );
        }
    }
}