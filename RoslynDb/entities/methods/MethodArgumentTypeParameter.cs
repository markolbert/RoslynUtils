using J4JSoftware.EFCoreUtilities;
using J4JSoftware.Roslyn.entities;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace J4JSoftware.Roslyn
{
    [EntityConfiguration( typeof( MethodArgumentTypeParameterConfigurator ) )]
    public class MethodArgumentTypeParameter : MethodArgumentBase, ISynchronized
    {
        public int TypeParmameterID { get; set; }
        public TypeParameterBase TypeParameter { get; set; }
    }

    internal class MethodArgumentTypeParameterConfigurator : EntityConfigurator<MethodArgumentTypeParameter>
    {
        protected override void Configure(EntityTypeBuilder<MethodArgumentTypeParameter> builder)
        {
            builder.HasOne( x => x.TypeParameter )
                .WithMany( x => x.MethodArguments )
                .HasPrincipalKey( x => x.ID )
                .HasForeignKey( x => x.TypeParmameterID );
        }
    }
}
