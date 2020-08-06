using J4JSoftware.EFCoreUtilities;
using J4JSoftware.Roslyn.entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace J4JSoftware.Roslyn
{
    public class ClosedTypeParameter : ISynchronized
    {
        public int ID { get; set; }
        public bool Synchronized { get; set; }
        public int TypeImplementationID { get; set; }
        public TypeImplementation TypeImplementation { get; set; }
        public int ParameterIndex { get; set; }
        public int ClosingTypeID { get; set; }
        public TypeDefinition ClosingType { get; set; }
    }

    internal class ClosedTypeParameterConfigurator : EntityConfigurator<ClosedTypeParameter>
    {
        protected override void Configure(EntityTypeBuilder<ClosedTypeParameter> builder)
        {
            builder.HasOne( x => x.ClosingType )
                .WithMany( x => x.GenericClosures )
                .HasForeignKey( x => x.ClosingTypeID )
                .HasPrincipalKey( x => x.ID );
        }
    }
}