using System.Collections.Generic;
using System.Xml.Serialization;
using J4JSoftware.EFCoreUtilities;
using J4JSoftware.Roslyn.Deprecated;
using J4JSoftware.Roslyn.entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace J4JSoftware.Roslyn
{
    [EntityConfiguration(typeof(TypeArgumentBaseConfigurator))]
    public class TypeArgumentBase : ISynchronized
    {
        protected TypeArgumentBase()
        {
        }

        public int ID { get; set; }
        public bool Synchronized { get; set; }
        public string Name { get; set; }
        public int Ordinal { get; set; }
    }

    internal class TypeArgumentBaseConfigurator : EntityConfigurator<TypeArgumentBase>
    {
        protected override void Configure(EntityTypeBuilder<TypeArgumentBase> builder)
        {
            builder.Property( x => x.Name )
                .IsRequired();
        }
    }

}
