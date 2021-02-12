﻿#region license

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
    [ EntityConfiguration( typeof(AttributeArgumentConfigurator) ) ]
    public class AttributeArgumentDb
    {
        public int ID { get; set; }
        public int AttributeID { get; set; }
        public AttributeDb Attribute { get; set; }

        public AttributeArgumentType ArgumentType { get; set; }
        public string? PropertyName { get; set; }
        public int? ConstructorArgumentOrdinal { get; set; }

        public string Value { get; set; }
    }

    internal class AttributeArgumentConfigurator : EntityConfigurator<AttributeArgumentDb>
    {
        protected override void Configure( EntityTypeBuilder<AttributeArgumentDb> builder )
        {
            builder.HasOne( x => x.Attribute )
                .WithMany( x => x.Arguments )
                .HasPrincipalKey( x => x.ID )
                .HasForeignKey( x => x.AttributeID );
        }
    }
}