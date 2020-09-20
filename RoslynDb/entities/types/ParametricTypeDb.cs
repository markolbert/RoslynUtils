using System;
using System.Collections.Generic;
using J4JSoftware.EFCoreUtilities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
#pragma warning disable 8602

namespace J4JSoftware.Roslyn
{
    [EntityConfiguration( typeof( ParametricTypeDbConfigurator ) )]
    public class ParametricTypeDb : BaseTypeDb, IParametricTypeEntity
    {
        public ParametricTypeConstraint Constraints { get; set; }

        public int? ContainingTypeID { get; set; }
        public BaseTypeDb? ContainingType { get; set; }

        int? IParametricTypeEntity.ContainerID
        {
            get => ContainingTypeID;
            set => ContainingTypeID = value;
        }

        object? IParametricTypeEntity.Container
        {
            get => ContainingType;

            set
            {
                if( value is BaseTypeDb typeDb )
                    ContainingType = typeDb;
                else throw new InvalidCastException( $"Expected a {typeof(BaseTypeDb)} but got a {value.GetType()}" );
            }
        }
    }

    internal class ParametricTypeDbConfigurator : EntityConfigurator<ParametricTypeDb>
    {
        protected override void Configure( EntityTypeBuilder<ParametricTypeDb> builder )
        {
            builder.HasOne( x => x.ContainingType )
                .WithMany( x => x.ParametricTypes )
                .HasPrincipalKey( x => x.SharpObjectID )
                .HasForeignKey( x => x.ContainingTypeID );

            builder.Property(x => x.Constraints)
                .HasConversion<string>();
        }
    }
}
