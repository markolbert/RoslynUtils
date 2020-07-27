using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using J4JSoftware.Logging;

namespace J4JSoftware.Roslyn
{
    public class SyntaxWalkers
    {
        private readonly IJ4JLogger _logger;
        private readonly List<ISyntaxWalker> _walkers;

        public SyntaxWalkers(
            IEnumerable<ISyntaxWalker> syntaxWalkers,
            IJ4JLogger logger
        )
        {
            _logger = logger;
            _logger.SetLoggedType( this.GetType() );

            var error = TopologicalSorter.CreateSequence( syntaxWalkers, out var walkers );

            if( error != null )
                _logger.Error( error );

            _walkers = walkers ?? new List<ISyntaxWalker>();
        }

        public bool Process( List<CompiledProject> compResults )
        {
            var allOkay = true;

            foreach( var walker in _walkers )
            {
                allOkay &= walker.Process( compResults );
            }

            return allOkay;
        }
    }
}