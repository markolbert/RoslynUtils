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
#pragma warning disable 8603

namespace J4JSoftware.Roslyn
{
    [ EntityConfiguration( typeof(NamespaceConfigurator) ) ]
    public class NamespaceDb : ISharpObject
    {
        public List<AssemblyNamespaceDb>? AssemblyNamespaces { get; set; }
        public List<BaseTypeDb>? Types { get; set; }
        public int SharpObjectID { get; set; }
        public SharpObject SharpObject { get; set; }
    }

    internal class NamespaceConfigurator : EntityConfigurator<NamespaceDb>
    {
        protected override void Configure( EntityTypeBuilder<NamespaceDb> builder )
        {
            builder.HasKey( x => x.SharpObjectID );

            builder.HasOne( x => x.SharpObject )
                .WithOne( x => x.Namespace )
                .HasPrincipalKey<SharpObject>( x => x.ID )
                .HasForeignKey<NamespaceDb>( x => x.SharpObjectID );

            builder.HasMany( x => x.AssemblyNamespaces )
                .WithOne( x => x.Namespace )
                .HasForeignKey( x => x.NamespaceID )
                .HasPrincipalKey( x => x.SharpObjectID );

            builder.HasMany( x => x.Types )
                .WithOne( x => x.Namespace )
                .HasForeignKey( x => x.NamespaceID )
                .HasPrincipalKey( x => x.SharpObjectID );
        }
    }
}