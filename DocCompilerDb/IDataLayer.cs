using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.DocCompiler
{
    public interface IDataLayer
    {
        void Deprecate();

        void SaveChanges();

        void UpdateAssembly( IProjectInfo projInfo );
        bool UpdateCodeFile( IScannedFile scannedFile );
        bool UpdateNamespace( SyntaxNode node );
    }
}
