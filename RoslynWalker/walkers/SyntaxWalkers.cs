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

            //var walkers = syntaxWalkers!.ToList();

            //_walkers = TopologicalSorter.CreateSequence( walkers, w =>
            //{
            //    var retVal = new List<ISyntaxWalker>();

            //    foreach( var depAttr in w.GetType()
            //        .GetCustomAttributes<RoslynProcessorAttribute>() )
            //    {
            //        var predecessorWalker = walkers.FirstOrDefault( w => w.GetType() == depAttr.PredecessorType );

            //        if( predecessorWalker == null )
            //            throw new ArgumentOutOfRangeException(
            //                $"Could not find Type {depAttr.PredecessorType.Name} updater in provided collection" );

            //        retVal.Add( predecessorWalker );
            //    }

            //    return retVal;
            //} ) ?? new List<ISyntaxWalker>();

            //if (_walkers.Count == 0)
            //    _logger.Error<Type, Type>(
            //        "Couldn't determine {0} sequence. Check your {1} attributes", 
            //        typeof(ISyntaxWalker),
            //        typeof(RoslynProcessorAttribute) );

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