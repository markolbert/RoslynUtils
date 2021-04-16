using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using J4JSoftware.EFCoreUtilities;
using J4JSoftware.Logging;
using Microsoft.EntityFrameworkCore;
#pragma warning disable 8618

namespace J4JSoftware.DocCompiler
{
    public class DocDbContext : DbContext
    {
        private readonly IJ4JLogger? _logger;

        public DocDbContext( 
            DbContextOptions<DocDbContext> contextOptions,
            DatabaseConfig dbConfig,
            IJ4JLogger? logger
            )
            : base( contextOptions )
        {
            DbConfig = dbConfig;

            _logger = logger;
            _logger?.SetLoggedType( GetType() );
        }

        public DatabaseConfig DbConfig { get; }

        public DbSet<Assembly> Assemblies { get; set; }
        public DbSet<CodeFile> CodeFiles { get; set; }

        public DbSet<Namespace> Namespaces { get; set; }

        public DbSet<DocumentedType> DocumentedTypes { get; set; }
        public DbSet<ExternalType> ExternalTypes { get; set; }
        public DbSet<LocalType> LocalTypes { get; set; }

        public DbSet<TypeParameter> TypeParameters { get; set; }
        public DbSet<TypeConstraint> TypeConstraints { get; set; }
        public DbSet<TypeArgument> TypeArguments { get; set; }
        public DbSet<TypeReference> TypeReferences { get; set; }
        public DbSet<TypeAncestor> TypeAncestors { get; set; }

        public DbSet<Event> Events { get; set; }
        public DbSet<Field> Fields { get; set; }

        public DbSet<MethodArgument> MethodArguments { get; set; }
        public DbSet<Method> Methods { get; set; }

        public DbSet<PropertyArgument> PropertyArguments { get; set; }
        public DbSet<Property> Properties { get;set; }

        public DbSet<Author> Authors { get; set; }
        public DbSet<Documentation> Documentation { get; set; }
        public DbSet<DocumentationEntry> DocumentationEntries { get; set; }

        protected override void OnModelCreating( ModelBuilder modelBuilder )
        {
            base.OnModelCreating( modelBuilder );

            modelBuilder.ConfigureEntities( GetType().Assembly );
        }
    }
}
