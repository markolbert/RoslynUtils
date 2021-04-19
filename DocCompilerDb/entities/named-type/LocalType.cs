﻿#region license

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

using System;
using System.Collections.Generic;
using J4JSoftware.EFCoreUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
#pragma warning disable 8618

namespace J4JSoftware.DocCompiler
{
    [ EntityConfiguration( typeof(LocalTypeConfigurator) ) ]
    public class LocalType : NamedType
    {
        public int DeclaringTypeID { get; set; }
        public DocumentedType DeclaringType { get; set; }
        public int TypeParameterIndex { get; set; }
    }

    internal class LocalTypeConfigurator : EntityConfigurator<LocalType>
    {
        protected override void Configure( EntityTypeBuilder<LocalType> builder )
        {
            builder.HasIndex( x => new { x.DeclaringTypeID, x.TypeParameterIndex } )
                .IsUnique();

            builder.HasOne( x => x.DeclaringType )
                .WithMany( x => x.LocalTypes )
                .HasForeignKey( x => x.DeclaringTypeID )
                .HasPrincipalKey( x => x.ID );
        }
    }
}