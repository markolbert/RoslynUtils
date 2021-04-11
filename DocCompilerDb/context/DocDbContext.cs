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
        private readonly List<IQueryable> _deprecatable;
        private readonly IJ4JLogger? _logger;

        public DocDbContext( 
            DbContextOptions<DocDbContext> contextOptions,
            IJ4JLogger? logger
            )
            : base( contextOptions )
        {
            _deprecatable = GetDeprecatable().ToList();

            _logger = logger;
            _logger?.SetLoggedType( GetType() );
        }

        private List<IQueryable> GetDeprecatable()
        {
            return this.GetType().GetProperties().Where( p =>
                {
                    if( !p.PropertyType.IsGenericType 
                        || p.PropertyType.GetGenericTypeDefinition() != typeof(DbSet<>) )
                        return false;

                    var typeArgs = p.PropertyType.GetGenericArguments();

                    return typeArgs.Length == 1 && typeof(IDeprecation).IsAssignableFrom( typeArgs[ 0 ] );
                } )
                .Select( p => (IQueryable) p.GetValue( this )! )
                .ToList();
        }

        public DbSet<Assembly> Assemblies { get; set; }
        public DbSet<CodeFile> CodeFiles { get; set; }

        public DbSet<Namespace> Namespaces { get; set; }

        public DbSet<DocumentedType> DocumentedTypes { get; set; }
        public DbSet<ExternalType> ExternalTypes { get; set; }
        public DbSet<TypeParameter> TypeParameters { get; set; }
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

        public void Deprecate()
        {
            foreach( var dbSet in _deprecatable )
            {
                foreach( var item in dbSet )
                {
                    ( (IDeprecation) item ).Deprecated = true;
                }
            }

            SaveChanges();
        }

        protected override void OnModelCreating( ModelBuilder modelBuilder )
        {
            base.OnModelCreating( modelBuilder );

            modelBuilder.ConfigureEntities( GetType().Assembly );
        }
    }
}
