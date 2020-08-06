using J4JSoftware.EFCoreUtilities;
using J4JSoftware.Roslyn.entities;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace J4JSoftware.Roslyn
{
    [EntityConfiguration( typeof( MethodArgumentConfigurator ) )]
    public class MethodArgument : ISynchronized
    {
        protected MethodArgument()
        {
        }

        public int ID { get; set; }
        public int ParameterIndex { get; set; }
        public string Name { get; set; }
        public bool Synchronized { get; set; }
        public int DeclaringMethodID { get; set; }
        public Method DeclaringMethod { get; set; }

        public bool IsOptional { get; set; }
        public bool IsParams { get; set; }
        public bool IsThis { get; set; }
        public bool IsDiscard { get; set; }
        public RefKind ReferenceKind { get; set; }
        public string? DefaultText { get; set; }
    }

    internal class MethodArgumentConfigurator : EntityConfigurator<MethodArgument>
    {
        protected override void Configure(EntityTypeBuilder<MethodArgument> builder)
        {
            builder.HasOne(x => x.DeclaringMethod)
                .WithMany(x => x.Arguments)
                .HasForeignKey(x => x.DeclaringMethodID)
                .HasPrincipalKey(x => x.ID);

            builder.Property( x => x.ReferenceKind )
                .HasConversion( new EnumToNumberConverter<RefKind, int>() );

            builder.HasIndex(x => new { x.DeclaringMethodID, x.ParameterIndex });
        }
    }
}
