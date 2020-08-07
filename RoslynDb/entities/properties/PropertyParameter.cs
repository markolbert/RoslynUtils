using System.Collections.Generic;
using J4JSoftware.EFCoreUtilities;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace J4JSoftware.Roslyn
{
    [ EntityConfiguration( typeof(PropertyParameterConfigurator) ) ]
    public class PropertyParameter
    {
        protected PropertyParameter()
        {
        }

        public int ID { get; set; }
        public string Name { get; set; }
        public int PropertyID { get; set; }
        public Property Property { get; set; }
        public int ParameterIndex { get; set; }
    }

    internal class PropertyParameterConfigurator : EntityConfigurator<PropertyParameter>
    {
        protected override void Configure( EntityTypeBuilder<PropertyParameter> builder )
        {
            builder.HasOne( x => x.Property )
                .WithMany( x => x.Parameters )
                .HasPrincipalKey( x => x.ID )
                .HasForeignKey( x => x.PropertyID );
        }
    }

    public class TypeBase
    {
        protected TypeBase()
        {
        }

        public int ID { get; set; }
        public int ContainerID { get; set; }
    }

    public class ClosedTypeBase : TypeBase
    {
        protected ClosedTypeBase()
        {
        }

        public int ClosedTypeID { get; set; }
        public TypeDefinition ClosedType { get; set; }
    }

    public class GenericTypeBase : TypeBase
    {
        protected GenericTypeBase()
        {
        }

        public GenericConstraint Constraints { get; set; }
    }

    public class GenericTypeConstraint
    {
        protected GenericTypeConstraint()
        {
        }

        public int GenericTypeID { get; set; }
        public GenericTypeBase GenericType { get; set; }
        public int ConstrainingTypeID { get; set; }
        public TypeDefinition ConstrainingType { get; set; }
    }
}