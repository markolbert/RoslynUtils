using System.Collections.Generic;
using System.Xml.Serialization;
using J4JSoftware.EFCoreUtilities;
using J4JSoftware.Roslyn.Deprecated;
using J4JSoftware.Roslyn.entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace J4JSoftware.Roslyn
{
    [EntityConfiguration(typeof(TypeArgumentConfigurator))]
    public class TypeArgument : ISynchronized
    {
        protected TypeArgument()
        {
        }

        public bool Synchronized { get; set; }
        public int Ordinal { get; set; }
    }

    internal class TypeArgumentConfigurator : EntityConfigurator<TypeArgument>
    {
        protected override void Configure(EntityTypeBuilder<TypeArgument> builder)
        {
        }
    }

}
