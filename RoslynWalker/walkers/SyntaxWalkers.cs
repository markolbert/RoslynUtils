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

            var nodes = new HashSet<ISyntaxWalker>();
            var edges = new HashSet<(ISyntaxWalker start, ISyntaxWalker)>();

            var walkers = syntaxWalkers.ToList();

            foreach (var walker in walkers)
            {
                nodes.Add(walker);

                foreach (var depAttr in walker.GetType()
                    .GetCustomAttributes<PredecessorWalkerAttribute>())
                {
                    var predecessorWalker = walkers.FirstOrDefault( w => w.GetType() == depAttr.WalkerType );

                    if ( predecessorWalker == null )
                        throw new ArgumentOutOfRangeException(
                            $"Could not find Type {depAttr.WalkerType.Name} updater in provided {nameof(syntaxWalkers)} collection");

                    edges.Add( ( walker, predecessorWalker ) );
                }
            }

            _walkers = TopologicalSorter.Sort(nodes, edges).ToList();

            if (_walkers.Count == 0)
                _logger.Error<Type, Type>(
                    "Couldn't determine {0} sequence. Check your {1} attributes", 
                    typeof(ISyntaxWalker),
                    typeof(PredecessorWalkerAttribute) );
        }

        public bool Traverse( List<CompiledProject> compResults )
        {
            var allOkay = true;

            foreach( var walker in _walkers )
            {
                allOkay &= walker.Traverse( compResults );
            }

            return allOkay;
        }
    }
}