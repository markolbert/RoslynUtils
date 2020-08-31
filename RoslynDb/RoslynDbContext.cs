﻿using System;
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

        // assemblies and namespaces
        public DbSet<AssemblyDb> Assemblies { get; set; }
        public DbSet<InScopeAssemblyInfo> InScopeInfo { get; set; }
        public DbSet<NamespaceDb> Namespaces { get; set; }
        public DbSet<AssemblyNamespace> AssemblyNamespaces { get; set; }

        // type definition and implementation
        public DbSet<FixedTypeDb> FixedTypes { get; set; }
        public DbSet<GenericTypeDb> GenericTypes { get; set; }
        public DbSet<ParametricTypeDb> TypeParametricTypes { get; set; }
        public DbSet<MethodParametricTypeDb> MethodParametericTypes { get; set; }
        public DbSet<TypeAncestor> TypeAncestors { get; set; }

        public DbSet<MethodDb> Methods { get; set; }
        public DbSet<MethodArgument> MethodArguments { get; set; }
        public DbSet<MethodPlaceholderDb> PlaceholderMethods { get; set; }

        public DbSet<Property> Properties { get; set; }
        public DbSet<PropertyParameter> PropertyParameters { get; set; }

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
