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
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

#pragma warning disable 8618

namespace J4JSoftware.Roslyn
{
    [ EntityConfiguration( typeof(MethodArgumentConfigurator) ) ]
    public class ArgumentDb : ISharpObject
    {
        public int Ordinal { get; set; }

        public int MethodID { get; set; }
        public MethodDb Method { get; set; }

        public bool IsOptional { get; set; }
        public bool IsParams { get; set; }
        public bool IsThis { get; set; }
        public bool IsDiscard { get; set; }
        public RefKind ReferenceKind { get; set; }
        public string? DefaultText { get; set; }

        public int ArgumentTypeID { get; set; }
        public BaseTypeDb ArgumentType { get; set; }

        // list of attribute arguments which reference this argument
        public List<AttributeArgumentDb> AttributeArgumentReferences { get; set; }
        public int SharpObjectID { get; set; }
        public SharpObject SharpObject { get; set; }
    }

    internal class MethodArgumentConfigurator : EntityConfigurator<ArgumentDb>
    {
        protected override void Configure( EntityTypeBuilder<ArgumentDb> builder )
        {
            builder.HasKey( x => x.SharpObjectID );

            builder.HasOne( x => x.ArgumentType )
                .WithMany( x => x.MethodArguments )
                .HasPrincipalKey( x => x.SharpObjectID )
                .HasForeignKey( x => x.ArgumentTypeID );

            builder.HasOne( x => x.Method )
                .WithMany( x => x.Arguments )
                .HasPrincipalKey( x => x.SharpObjectID )
                .HasForeignKey( x => x.MethodID );
        }
    }
}