using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using J4JSoftware.EFCoreUtilities;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
#pragma warning disable 8618

namespace J4JSoftware.Roslyn
{
    public class RoslynDbContext : DbContext
    {
        private readonly IDbContextFactoryConfiguration _config;

        public RoslynDbContext(IDbContextFactoryConfiguration config)
        {
            _config = config ?? throw new NullReferenceException(nameof(config));
        }

        // core entities
        public DbSet<Assembly> Assemblies { get; set; }
        public DbSet<InScopeAssemblyInfo> InScopeInfo { get; set; }
        public DbSet<Namespace> Namespaces { get; set; }
        public DbSet<NamedType> NamedTypes { get; set; }
        public DbSet<GenericTypeConstraint> GenericConstraints { get; set; }

        // linking (many-to-many) entities
        public DbSet<AssemblyNamespace> AssemblyNamespaces { get; set; }
        public DbSet<TypeGenericParameter> TypeGenericParameters { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);

            // we open the connection, and use the opened connection to initialize the entity
            // framework via optionsBuilder, to preserve the UDF configuration
            var connection = new SqliteConnection($"DataSource={_config.DatabasePath}");
            //var connection = new SqliteConnection($"DataSource=:memory:");
            connection.Open();

            optionsBuilder.UseSqlite(connection);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ConfigureEntities(this.GetType().Assembly);
        }
    }
}
