using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using J4JSoftware.EFCoreUtilities;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
#pragma warning disable 8618
#pragma warning disable 8603

namespace J4JSoftware.Roslyn
{
    [EntityConfiguration( typeof( TypeDbConfigurator ) )]
    [Table("Types")]
    public abstract class BaseTypeDb : ISharpObject
    {
        public int SharpObjectID { get; set; }
        public SharpObject SharpObject { get; set; }

        public TypeKind TypeKind { get; set; }
        public Accessibility Accessibility { get; set; }
        public bool InDocumentationScope { get; set; }

        public int NamespaceID { get; set; }
        public NamespaceDb Namespace { get; set; }

        public int AssemblyID { get; set; }
        public AssemblyDb Assembly { get; set; }

        // parametric types, if any, defined for this type
        public List<ParametricTypeDb> ParametricTypes { get; set; }

        // list of TypeArguments where this type is referenced as a closing type
        public List<TypeArgumentDb> TypeArgumentReferences { get; set; }

        // list of return types referencing this type definition
        public List<MethodDb> ReturnTypes { get; set; }

        // list of method arguments referencing this type definition
        public List<ArgumentDb> MethodArguments { get; set; }

        // list of properties having a return value equal to this type
        public List<PropertyDb> PropertyTypes { get; set; }

        // list of types implemented by this type (including the type it is descended from)
        public List<TypeAncestorDb> AncestorTypes { get; set; }

        // list of fields having this type
        public List<FieldDb> FieldTypes { get; set; }

        // list of array types having this type for elements
        public List<ArrayTypeDb> ArrayTypes { get; set; }

        // list of events having this type
        public List<EventDb> EventTypes { get; set; }
        }

    internal class TypeDbConfigurator : EntityConfigurator<BaseTypeDb>
    {
        protected override void Configure( EntityTypeBuilder<BaseTypeDb> builder )
        {
            builder.HasKey(x => x.SharpObjectID);

            builder.HasOne( x => x.Namespace )
                .WithMany( x => x.Types )
                .HasForeignKey( x => x.NamespaceID )
                .HasPrincipalKey( x => x.SharpObjectID );

            builder.HasOne(x => x.Assembly)
                .WithMany(x => x.Types)
                .HasForeignKey(x => x.AssemblyID)
                .HasPrincipalKey(x => x.SharpObjectID);

            builder.Property( x => x.Accessibility )
                .HasConversion<string>();

            builder.Property(x=>x.TypeKind  )
                .HasConversion<string>();
        }
    }
}
