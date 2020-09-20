using System.Collections.Generic;
using J4JSoftware.EFCoreUtilities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
#pragma warning disable 8618

namespace J4JSoftware.Roslyn
{
    [EntityConfiguration(typeof(DefinedTypeDbConfigurator))]
    public abstract class ImplementableTypeDb : BaseTypeDb
    {
        public DeclarationModifier DeclarationModifier { get; set; }

        // list of methods defined for this type
        public List<MethodDb> Methods { get; set; }

        // list of properties defined for this type
        public List<PropertyDb> Properties { get; set; }

        // list of property arguments referencing this type
        public List<PropertyParameterDb> PropertyParameters { get; set; }

        // list of fields for this type
        public List<FieldDb> Fields { get; set; }
    }

    internal class DefinedTypeDbConfigurator : EntityConfigurator<ImplementableTypeDb>
    {
        protected override void Configure( EntityTypeBuilder<ImplementableTypeDb> builder )
        {
            builder.Property(x => x.DeclarationModifier)
                .HasConversion<string>();
        }
    }
}
