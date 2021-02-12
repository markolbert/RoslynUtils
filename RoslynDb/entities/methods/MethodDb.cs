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
#pragma warning disable 8603

namespace J4JSoftware.Roslyn
{
    [ EntityConfiguration( typeof(MethodDbConfigurator) ) ]
    public class MethodDb : ISharpObject
    {
        public MethodKind Kind { get; set; }
        public Accessibility Accessibility { get; set; }
        public DeclarationModifier DeclarationModifier { get; set; }

        public int DefiningTypeID { get; set; }
        public ImplementableTypeDb DefiningType { get; set; }

        public int ReturnTypeID { get; set; }
        public BaseTypeDb ReturnType { get; set; }

        public bool ReturnsByRef { get; set; }
        public bool ReturnsByRefReadOnly { get; set; }

        public bool IsAbstract { get; set; }
        public bool IsExtern { get; set; }
        public bool IsOverride { get; set; }
        public bool IsReadOnly { get; set; }
        public bool IsSealed { get; set; }
        public bool IsStatic { get; set; }
        public bool IsVirtual { get; set; }


        // list of method arguments
        public List<ArgumentDb> Arguments { get; set; }

        public List<ParametricMethodTypeDb> ParametricTypes { get; set; }

        // list of event add methods which reference this method
        public List<EventDb> AddEvents { get; set; }

        // list of event remove methods which reference this method
        public List<EventDb> RemoveEvents { get; set; }
        public int SharpObjectID { get; set; }
        public SharpObject SharpObject { get; set; }
    }

    internal class MethodDbConfigurator : EntityConfigurator<MethodDb>
    {
        protected override void Configure( EntityTypeBuilder<MethodDb> builder )
        {
            builder.HasKey( x => x.SharpObjectID );

            builder.HasOne( x => x.DefiningType )
                .WithMany( x => x.Methods )
                .HasForeignKey( x => x.DefiningTypeID )
                .HasPrincipalKey( x => x.SharpObjectID );

            builder.HasOne( x => x.ReturnType )
                .WithMany( x => x.ReturnTypes )
                .HasPrincipalKey( x => x.SharpObjectID )
                .HasForeignKey( x => x.ReturnTypeID );

            builder.HasOne( x => x.SharpObject )
                .WithOne( x => x.Method )
                .HasPrincipalKey<SharpObject>( x => x.ID )
                .HasForeignKey<MethodDb>( x => x.SharpObjectID );

            builder.Property( x => x.Accessibility )
                .HasConversion<string>();

            builder.Property( x => x.DeclarationModifier )
                .HasConversion<string>();

            builder.Property( x => x.Kind )
                .HasConversion<string>();
        }
    }
}