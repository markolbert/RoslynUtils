using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using J4JSoftware.EFCoreUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace J4JSoftware.DocCompiler
{
    public class DocDbContext : DbContext
    {
        public DocDbContext( DbContextOptions<DocDbContext> contextOptions )
            : base( contextOptions )
        {

        }

        public DbSet<Assembly> Assemblies { get; set; }
        public DbSet<CodeFile> CodeFiles { get; set; }

        public DbSet<Namespace> Namespaces { get; set; }
        public DbSet<Using> Usings { get; set; }

        public DbSet<NamedType> NamedTypes { get; set; }

        public DbSet<Event> Events { get; set; }
        public DbSet<Field> Fields { get; set; }

        public DbSet<MethodArgument> MethodArguments { get; set; }
        public DbSet<Method> Methods { get; set; }

        public DbSet<PropertyArgument> PropertyArguments { get; set; }
        public DbSet<Property> Properties { get;set; }

        protected override void OnModelCreating( ModelBuilder modelBuilder )
        {
            base.OnModelCreating( modelBuilder );

            modelBuilder.ConfigureEntities( GetType().Assembly );
        }
    }

    public class DocDbContextFactory : IDesignTimeDbContextFactory<DocDbContext>
    {
        public DocDbContext CreateDbContext( string[] args )
        {
            var optionsBuilder = new DbContextOptionsBuilder<DocDbContext>();
            optionsBuilder.UseSqlite($"Data Source={args[0]}");

            return new DocDbContext( optionsBuilder.Options );
        }
    }
}
