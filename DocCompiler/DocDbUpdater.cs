using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.DocCompiler
{
    public class DocDbUpdater : IDocDbUpdater
    {
        private readonly IDataLayer _dataLayer;
        private readonly IJ4JLogger? _logger;

        public DocDbUpdater(
            IDataLayer dataLayer,
            IJ4JLogger? logger
        )
        {
            _dataLayer = dataLayer;

            _logger = logger;
            _logger?.SetLoggedType( GetType() );
        }

        public bool UpdateDatabase( IDocScanner docScanner )
        {
            _dataLayer.Deprecate();

            UpdateAssemblies( docScanner.Projects );

            if( !UpdateCodeFiles( docScanner.ScannedFiles ) )
                return false;

            if( !UpdateNamespaces( docScanner ) )
                return false;

            return true;
        }

        private void UpdateAssemblies( IEnumerable<IProjectInfo> projects )
        {
            foreach( var projInfo in projects )
            {
                _dataLayer.UpdateAssembly( projInfo );
            }
        }

        private bool UpdateCodeFiles( IEnumerable<IScannedFile> scannedFiles )
        {
            if( !scannedFiles.All( scannedFile => _dataLayer.UpdateCodeFile( scannedFile ) ) )
                return false;

            return true;
        }

        private bool UpdateNamespaces( IDocScanner docScanner )
        {
            if( !docScanner.Namespaces.All( nsNode => _dataLayer.UpdateNamespace( nsNode ) ) )
                return false;

            return true;
        }
    }
}
