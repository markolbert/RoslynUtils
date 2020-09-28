using System.Collections.Generic;
using J4JSoftware.EFCoreUtilities;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
#pragma warning disable 8618

namespace J4JSoftware.Roslyn
{
    [EntityConfiguration(typeof(DefinedTypeDbConfigurator))]
    public abstract class ImplementableTypeDb : BaseTypeDb
    {
        public DeclarationModifier DeclarationModifier { get; set; }
        public bool IsDelegate => TypeKind == TypeKind.Delegate;

        public List<MethodDb> Methods { get; set; }
        public List<PropertyDb> Properties { get; set; }
        public List<PropertyParameterDb> PropertyParameters { get; set; }
        public List<FieldDb> Fields { get; set; }
        public List<EventDb> Events { get; set; }
    }

    internal class DefinedTypeDbConfigurator : EntityConfigurator<ImplementableTypeDb>
    {
        protected override void Configure( EntityTypeBuilder<ImplementableTypeDb> builder )
        {
            builder.Property(x => x.DeclarationModifier)
                .HasConversion<string>();

            builder.Ignore(x => x.IsDelegate);
        }
    }
}
