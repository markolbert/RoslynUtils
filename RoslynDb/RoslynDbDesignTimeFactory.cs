using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using J4JSoftware.EFCoreUtilities;

namespace J4JSoftware.Roslyn
{
    public class RoslynDbDesignTimeFactory : DesignTimeFactory<RoslynDbContext>
    {
        protected override IDbContextFactoryConfiguration GetDatabaseConfiguration() =>
            new RoslynDbContextFactoryConfiguration();
    }
}
