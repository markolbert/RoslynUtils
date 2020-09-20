using System;
using J4JSoftware.EFCoreUtilities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
#pragma warning disable 8602

namespace J4JSoftware.Roslyn
{
    [EntityConfiguration( typeof( ParametricMethodTypeDbConfigurator ) )]
    public class ParametricMethodTypeDb : BaseTypeDb, IParametricTypeEntity
    {
        public ParametricTypeConstraint Constraints { get; set; }
        public int? ContainingMethodID { get; set; }
        public MethodDb? ContainingMethod { get; set; }

        int? IParametricTypeEntity.ContainerID
        {
            get => ContainingMethodID;
            set => ContainingMethodID = value;
        }

        object? IParametricTypeEntity.Container
        {
            get => ContainingMethod;

            set
            {
                if (value is MethodDb methodDb)
                    ContainingMethod = methodDb;
                else throw new InvalidCastException($"Expected a {typeof(MethodDb)} but got a {value.GetType()}");
            }
        }
    }

    internal class ParametricMethodTypeDbConfigurator : EntityConfigurator<ParametricMethodTypeDb>
    {
        protected override void Configure( EntityTypeBuilder<ParametricMethodTypeDb> builder )
        {
            builder.HasOne( x => x.ContainingMethod )
                .WithMany( x => x.ParametricTypes )
                .HasPrincipalKey( x => x.SharpObjectID )
                .HasForeignKey( x => x.ContainingMethodID );

            builder.Property(x => x.Constraints)
                .HasConversion<string>();
        }
    }
}
