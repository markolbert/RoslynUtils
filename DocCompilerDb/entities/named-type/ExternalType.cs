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

using System;
using System.Collections.Generic;
using J4JSoftware.EFCoreUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace J4JSoftware.DocCompiler
{
    [ EntityConfiguration( typeof(ExternalTypeConfigurator) ) ]
    public class ExternalType : NamedType
    {
        public string? ExternalUrl { get; set; }
        public int NumTypeParameters { get; set; }
        public ICollection<Namespace>? PossibleNamespaces { get; set; }
    }

    internal class ExternalTypeConfigurator : EntityConfigurator<ExternalType>
    {
        protected override void Configure( EntityTypeBuilder<ExternalType> builder )
        {
            builder.HasIndex( x => new { x.Name, x.NumTypeParameters } )
                .IsUnique();

            builder.HasMany( x => x.PossibleNamespaces )
                .WithMany( x => x.ExternalTypes );
        }
    }
}