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
    [ EntityConfiguration( typeof(DefinedTypeDbConfigurator) ) ]
    public abstract class ImplementableTypeDb : BaseTypeDb
    {
        public DeclarationModifier DeclarationModifier { get; set; }
        public bool IsDelegate => TypeKind == TypeKind.Delegate;

        public List<MethodDb> Methods { get; set; }
        public List<PropertyDb> Properties { get; set; }
        public List<PropertyParameterDb> PropertyParameters { get; set; }
        public List<FieldDb> Fields { get; set; }

        public List<EventDb> Events { get; set; }

        // list of attributes referencing this type (which must be an attribute)
        public List<AttributeDb> AttributeReferences { get; set; }
    }

    internal class DefinedTypeDbConfigurator : EntityConfigurator<ImplementableTypeDb>
    {
        protected override void Configure( EntityTypeBuilder<ImplementableTypeDb> builder )
        {
            builder.Property( x => x.DeclarationModifier )
                .HasConversion<string>();

            builder.Ignore( x => x.IsDelegate );
        }
    }
}