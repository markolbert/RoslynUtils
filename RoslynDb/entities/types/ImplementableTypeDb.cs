using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using J4JSoftware.EFCoreUtilities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace J4JSoftware.Roslyn
{
    [EntityConfiguration(typeof(DefinedTypeDbConfigurator))]
    public class ImplementableTypeDb : TypeDb
    {
        protected ImplementableTypeDb()
        {
        }

        public DeclarationModifier DeclarationModifier { get; set; }

        // list of TypeArguments for this type
        public List<TypeArgumentDb> TypeArguments { get; set; }

        // list of TypeArguments where this type is referenced as a closing type
        public List<TypeArgumentDb> TypeArgumentReferences { get; set; }
        
        // list of methods defined for this type
        public List<MethodDb> Methods { get; set; }

        // list of properties defined for this type
        public List<PropertyDb> Properties { get; set; }

        // list of property arguments referencing this type
        public List<PropertyParameterDb> PropertyParameters { get; set; }
    }

    internal class DefinedTypeDbConfigurator : EntityConfigurator<ImplementableTypeDb>
    {
        protected override void Configure( EntityTypeBuilder<ImplementableTypeDb> builder )
        {
            builder.Property(x => x.DeclarationModifier)
                .HasConversion(new EnumToNumberConverter<DeclarationModifier, int>());

        }
    }
}
