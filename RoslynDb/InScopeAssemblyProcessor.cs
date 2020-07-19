using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using J4JSoftware.Logging;
using Microsoft.EntityFrameworkCore;

namespace J4JSoftware.Roslyn
{
    public class InScopeAssemblyProcessor : IInScopeAssemblyProcessor
    {
        private readonly RoslynDbContext _dbContext;
        private readonly Func<IJ4JLogger> _loggerFactory;
        private readonly IJ4JLogger _logger;

        public InScopeAssemblyProcessor(
            RoslynDbContext dbContext,
            Func<IJ4JLogger> loggerFactory
        )
        {
            _dbContext = dbContext;

            _loggerFactory = loggerFactory;

            _logger = _loggerFactory();
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

        public bool Finalize() => true;

        public bool Synchronize( List<string> projFiles )
        {
            var allOkay = true;

            foreach( var projFile in projFiles )
            {
                var projLib = new ProjectLibrary( projFile, _loggerFactory );

                var dbAssembly = _dbContext.Assemblies
                    .Include(a => a.InScopeInfo)
                    .FirstOrDefault(a => a.Name == projLib.AssemblyName);

                if (dbAssembly == null)
                {
                    _logger.Error<string>("Couldn't find entry in database for Assembly '{0}'", projLib.AssemblyName!);
                    continue;
                }

                dbAssembly.InScopeInfo ??= new InScopeAssemblyInfo();

                dbAssembly.InScopeInfo.TargetFrameworksText = projLib.TargetFrameworks.ToString();
            }



            return allOkay;
        }
    }
}
