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

using System.Collections.Generic;
using J4JSoftware.EFCoreUtilities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
#pragma warning disable 8618

namespace J4JSoftware.DocCompiler
{
    [EntityConfiguration(typeof(MethodConfigurator))]
    public class Method 
    {
        public int ID { get;set; }
        public string Name { get; set; }
        public string FullyQualifiedName { get; set; }
        public bool Deprecated { get; set; }

        public Accessibility Accessibility { get; set; }
        public bool IsAbstract { get; set; }
        public bool IsNew { get; set; }
        public bool IsVirtual { get; set; }
        public bool IsOverride { get; set; }
        public bool IsHidden { get; set; }
        public bool IsStatic { get; set; }

        public ICollection<DocumentedType> DeclaredIn { get; set; }
        public ICollection<MethodArgument> Arguments { get; set; }
        public int ReturnTypeID { get; set; }
        public TypeReference ReturnType { get; set; }
        public Documentation Documentation { get; set; }
    }

    internal class MethodConfigurator : EntityConfigurator<Method>
    {
        protected override void Configure( EntityTypeBuilder<Method> builder )
        {
            builder.HasIndex( x => x.FullyQualifiedName )
                .IsUnique();

            builder.HasMany( x => x.DeclaredIn )
                .WithMany( x => x.Methods );

            builder.HasOne( x => x.ReturnType )
                .WithMany( x => x.MethodReturnTypes )
                .HasForeignKey( x => x.ReturnTypeID )
                .HasPrincipalKey( x => x.ID );
        }
    }
}