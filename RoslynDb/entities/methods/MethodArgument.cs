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
        public int Ordinal { get; set; }
        public string Name { get; set; }
        public bool Synchronized { get; set; }

        public int DeclaringMethodID { get; set; }
        public MethodDb DeclaringMethod { get; set; }

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
        }
    }
}
