using J4JSoftware.EFCoreUtilities;
using J4JSoftware.Roslyn.entities;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace J4JSoftware.Roslyn
{
    [EntityConfiguration( typeof( MethodArgumentConfigurator ) )]
    public class MethodArgument : MethodArgumentBase, ISynchronized
    {
        public int ArgumentTypeId { get; set; }
        public TypeDefinition ArgumentType { get; set; }
    }

    internal class MethodArgumentConfigurator : EntityConfigurator<MethodArgument>
    {
        protected override void Configure(EntityTypeBuilder<MethodArgument> builder)
        {
            builder.HasOne( x => x.ArgumentType )
                .WithMany( x => x.MethodParameters )
                .HasPrincipalKey( x => x.ID )
                .HasForeignKey( x => x.ArgumentTypeId );
        }
    }
}
