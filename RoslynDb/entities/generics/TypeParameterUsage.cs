using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using J4JSoftware.EFCoreUtilities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace J4JSoftware.Roslyn
{
    [EntityConfiguration(typeof(TypeParameterUsageConfigurator))]
    public class TypeParameterUsage
    {
        protected TypeParameterUsage()
        {
        }

        public int ID { get; set; }
        public string Name { get; set; }
        public int Ordinal { get; set; }
        public int TypeParameterID { get; set; }
        public TypeParameter TypeParameter { get; set; }
    }

    internal class TypeParameterUsageConfigurator : EntityConfigurator<TypeParameterUsage>
    {
        protected override void Configure(EntityTypeBuilder<TypeParameterUsage> builder)
        {
            builder.HasOne(x => x.TypeParameter)
                .WithMany(x => x.References)
                .HasPrincipalKey(x => x.ID)
                .HasForeignKey(x => x.TypeParameterID);

            builder.Property(x => x.Name)
                .IsRequired();
        }
    }
}
