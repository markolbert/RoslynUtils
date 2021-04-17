using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using J4JSoftware.Logging;
using J4JSoftware.Utilities;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.DocCompiler
{
    public class DocDbUpdater : IDocDbUpdater
    {
        private readonly List<IEntityProcessor> _processors;
        private readonly IJ4JLogger? _logger;

        public DocDbUpdater(
            IEnumerable<IEntityProcessor> processors,
            TopologicalSortFactory tsFactory,
            IJ4JLogger? logger
        )
        {
            if( !tsFactory.CreateSortedList(processors, out var temp))
                throw new ArgumentException( "Could not topologically sort EntityProcessor<,> collection" );

            _processors = temp!;

            _logger = logger;
            _logger?.SetLoggedType( GetType() );
        }

        public bool UpdateDatabase( IDocScanner docScanner ) =>
            _processors.All( processor => processor.UpdateDb( docScanner ) );
    }
}
