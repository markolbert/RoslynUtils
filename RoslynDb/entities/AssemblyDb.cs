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
using System.Collections.Generic;
using J4JSoftware.EFCoreUtilities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

#pragma warning disable 8618
#pragma warning disable 8602
#pragma warning disable 8603

namespace J4JSoftware.Roslyn
{
    [ EntityConfiguration( typeof(AssemblyConfigurator) ) ]
    public class AssemblyDb : ISharpObject
    {
        public string DotNetVersionText { get; set; } = "0.0.0.0";

        public Version DotNetVersion
        {
            get => Version.TryParse( DotNetVersionText, out var version )
                ? version
                : new Version( 0, 0, 0, 0 );

            set => DotNetVersionText = value.ToString();
        }

        public InScopeAssemblyInfo? InScopeInfo { get; set; }

        public List<AssemblyNamespaceDb>? AssemblyNamespaces { get; set; }
        public List<BaseTypeDb>? Types { get; set; }
        public int SharpObjectID { get; set; }
        public SharpObject SharpObject { get; set; }
    }

    internal class AssemblyConfigurator : EntityConfigurator<AssemblyDb>
    {
        protected override void Configure( EntityTypeBuilder<AssemblyDb> builder )
        {
            builder.HasKey( x => x.SharpObjectID );

            builder.HasOne( x => x.SharpObject )
                .WithOne( x => x.Assembly )
                .HasPrincipalKey<SharpObject>( x => x.ID )
                .HasForeignKey<AssemblyDb>( x => x.SharpObjectID );

            builder.Ignore( x => x.DotNetVersion );

            builder.HasMany( x => x.AssemblyNamespaces )
                .WithOne( x => x.Assembly )
                .HasForeignKey( x => x.AssemblyID )
                .HasPrincipalKey( x => x.SharpObjectID );
        }
    }
}