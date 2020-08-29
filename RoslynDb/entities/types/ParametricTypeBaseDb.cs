using System;
using System.Collections.Generic;
using System.Text;
using J4JSoftware.EFCoreUtilities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace J4JSoftware.Roslyn
{
    [EntityConfiguration(typeof(TypeParametricTypeBaseDbConfigurator))]
    public class ParametricTypeBaseDb : TypeDb
    {
        protected ParametricTypeBaseDb()
        {
        }

        public ParametricTypeConstraint Constraints { get; set; }
    }

    internal class TypeParametricTypeBaseDbConfigurator : EntityConfigurator<ParametricTypeBaseDb>
    {
        protected override void Configure(EntityTypeBuilder<ParametricTypeBaseDb> builder)
        {
            builder.Property(x => x.Constraints)
                .HasConversion(new EnumToNumberConverter<ParametricTypeConstraint, int>());
        }
    }

}
