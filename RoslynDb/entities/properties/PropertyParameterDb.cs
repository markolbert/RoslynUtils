using J4JSoftware.EFCoreUtilities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace J4JSoftware.Roslyn
{
    [ EntityConfiguration( typeof(PropertyParameterConfigurator) ) ]
    public class PropertyParameterDb : ISynchronized
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public int Ordinal { get; set; }
        public bool Synchronized { get; set; }

        public int PropertyID { get; set; }
        public PropertyDb Property { get; set; }

        public int ParameterTypeID { get; set; }
        public ImplementableTypeDb ParameterType { get; set; }

        public bool IsAbstract { get; set; }
        public bool IsExtern { get; set; }
        public bool IsOverride { get; set; }
        public bool IsSealed { get; set; }
        public bool IsStatic { get; set; }
        public bool IsVirtual { get; set; }
    }

    internal class PropertyParameterConfigurator : EntityConfigurator<PropertyParameterDb>
    {
        protected override void Configure( EntityTypeBuilder<PropertyParameterDb> builder )
        {
            builder.HasOne( x => x.Property )
                .WithMany( x => x.Parameters )
                .HasPrincipalKey( x => x.SharpObjectID )
                .HasForeignKey( x => x.PropertyID );

            builder.HasOne( x => x.ParameterType )
                .WithMany( x => x.PropertyParameters )
                .HasPrincipalKey( x => x.SharpObjectID )
                .HasForeignKey( x => x.ParameterTypeID );
        }
    }
}