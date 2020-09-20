using System.Collections.Generic;
using System.Linq;
using J4JSoftware.Logging;
using Microsoft.EntityFrameworkCore;

namespace J4JSoftware.Roslyn
{
    public class InScopeAssemblyProcessor : IInScopeAssemblyProcessor
    {
        private readonly EntityFactories _factories;
        private readonly IRoslynDataLayer _dataLayer;
        private readonly IJ4JLogger _logger;

        public InScopeAssemblyProcessor(
            EntityFactories factories,
            IJ4JLogger logger
        )
        {
            _factories = factories;

            _logger = logger;
            _logger.SetLoggedType( this.GetType() );
        }

        public bool Initialize()
        {
            _dataLayer.MarkUnsynchronized<InScopeAssemblyInfo>();

            return true;
        }

        public bool Synchronize( IEnumerable<CompiledProject> projects )
        {
            var allOkay = true;

            foreach( var library in projects )
            {
                var dbAssembly = _factories.DbContext.Assemblies
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

            _factories.DbContext.SaveChanges();

            return allOkay;
        }
    }
}
