using J4JSoftware.EFCoreUtilities;
using J4JSoftware.Roslyn.entities;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace J4JSoftware.Roslyn
{
    [EntityConfiguration( typeof( MethodArgumentBaseConfigurator ) )]
    public class MethodArgumentBase : ISynchronized
    {
        protected MethodArgumentBase()
        {
        }

        public int Ordinal { get; set; }
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

    internal class MethodArgumentBaseConfigurator : EntityConfigurator<MethodArgumentBase>
    {
        protected override void Configure(EntityTypeBuilder<MethodArgumentBase> builder)
        {
            builder.HasOne(x => x.DeclaringMethod)
                .WithMany(x => x.Arguments)
                .HasForeignKey(x => x.DeclaringMethodID)
                .HasPrincipalKey(x => x.ID);

            builder.Property( x => x.ReferenceKind )
                .HasConversion( new EnumToNumberConverter<RefKind, int>() );

            builder.HasKey( x => new { x.DeclaringMethodID, ParameterIndex = x.Ordinal } );
        }
    }
}
