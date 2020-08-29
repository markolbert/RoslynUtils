using System.Collections.Generic;
using J4JSoftware.EFCoreUtilities;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace J4JSoftware.Roslyn
{
    [ EntityConfiguration( typeof(PropertyParameterConfigurator) ) ]
    public class PropertyParameter
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public int Ordinal { get; set; }

        public int PropertyID { get; set; }
        public Property Property { get; set; }

        public int ParameterTypeID { get; set; }
        public FixedTypeDb ParameterType { get; set; }
    }

    internal class PropertyParameterConfigurator : EntityConfigurator<PropertyParameter>
    {
        protected override void Configure( EntityTypeBuilder<PropertyParameter> builder )
        {
            builder.HasOne( x => x.Property )
                .WithMany( x => x.Parameters )
                .HasPrincipalKey( x => x.ID )
                .HasForeignKey( x => x.PropertyID );

            builder.HasOne( x => x.ParameterType )
                .WithMany( x => x.PropertyParameters )
                .HasPrincipalKey( x => x.ID )
                .HasForeignKey( x => x.ParameterTypeID );
        }
    }
}