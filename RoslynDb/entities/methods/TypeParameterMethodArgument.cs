using J4JSoftware.EFCoreUtilities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace J4JSoftware.Roslyn
{
    [EntityConfiguration(typeof(MethodArgumentTypeParameterConfigurator))]
    public class TypeParameterMethodArgument : MethodArgument
    {
        public int TypeParameterID { get; set; }
        public TypeParameter TypeParameter { get; set; }
    }

    internal class MethodArgumentTypeParameterConfigurator : EntityConfigurator<TypeParameterMethodArgument>
    {
        protected override void Configure(EntityTypeBuilder<TypeParameterMethodArgument> builder)
        {
            builder.HasOne( x => x.TypeParameter )
                .WithMany( x => x.MethodArguments )
                .HasPrincipalKey( x => x.ID )
                .HasForeignKey( x => x.TypeParameterID );

            builder.HasIndex( x => new { x.DeclaringMethodID, x.TypeParameterID } )
                .IsUnique();
        }
    }
}