﻿using System.Collections.Generic;
using System.Linq;
using J4JSoftware.Logging;
using Microsoft.EntityFrameworkCore;

namespace J4JSoftware.Roslyn
{
    public class InScopeAssemblyProcessor : IInScopeAssemblyProcessor
    {
        private readonly RoslynDbContext _dbContext;
        private readonly IJ4JLogger _logger;

        public InScopeAssemblyProcessor(
            RoslynDbContext dbContext,
            IJ4JLogger logger
        )
        {
            _dbContext = dbContext;

            _logger = logger;
            _logger.SetLoggedType( this.GetType() );
        }

        public bool Initialize()
        {
            // reset all InScopeInfo objects to unsync'd
            foreach( var inScope in _dbContext.InScopeInfo )
            {
                inScope.Synchronized = false;
            }

            _dbContext.SaveChanges();

            return true;
        }

        public bool Cleanup() => true;

        public bool Synchronize( List<CompiledProject> projects )
        {
            var allOkay = true;

            foreach( var library in projects )
            {
                var dbAssembly = _dbContext.Assemblies
                    .Include(a => a.InScopeInfo)
                    .Include(a=>a.SharpObject )
                    .FirstOrDefault(a => a.SharpObject.Name == library.AssemblyName);

                if (dbAssembly == null)
                {
                    _logger.Error<string>("Couldn't find entry in database for Assembly '{0}'", library.AssemblyName!);
                    allOkay = false;

                    continue;
                }

                dbAssembly.InScopeInfo ??= new InScopeAssemblyInfo();

                dbAssembly.InScopeInfo.TargetFrameworksText = library.TargetFrameworksText;
                dbAssembly.InScopeInfo.Authors = library.Authors;
                dbAssembly.InScopeInfo.Company = library.Company;
                dbAssembly.InScopeInfo.Copyright = library.Copyright;
                dbAssembly.InScopeInfo.Description = library.Description;
                dbAssembly.InScopeInfo.FileVersionText = library.FileVersionText;
                dbAssembly.InScopeInfo.PackageVersionText = library.PackageVersionText;
                dbAssembly.InScopeInfo.RootNamespace = library.RootNamespace;
            }

            _dbContext.SaveChanges();

            return allOkay;
        }
    }
}
