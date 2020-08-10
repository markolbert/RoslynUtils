using System;
using System.Collections.Generic;
using System.Linq;
using J4JSoftware.Logging;

namespace J4JSoftware.Roslyn
{
    public abstract class TopologicallySortedCollection<T> : ITopologicallySorted<T>
        where T : class, ITopologicalSort<T>
    {
        protected TopologicallySortedCollection(
            IEnumerable<T> items,
            IJ4JLogger logger
        )
        {
            Logger = logger;
            Logger.SetLoggedType( this.GetType() );

            var itemList = items.ToList();

            SetPredecessors( itemList );

            switch( itemList.Count( x => x.Predecessor == null ) )
            {
                case 0:
                    Logger.Error<Type>( "No root {0} defined", typeof(T) );
                    break;

                case 1:
                    // no op; desired result
                    break;

                default:
                    Logger.Error<Type>( "Multiple root {0} objects defined", typeof(T) );
                    break;
            }

            if( TopologicalSorter.Sort( itemList, out var result ) )
                ExecutionSequence = result!;
            else
            {
                Logger.Error<Type>( "Couldn't create execution sequence for {0}", typeof(T) );
                ExecutionSequence = new List<T>();
            }
        }

        protected IJ4JLogger Logger { get; }

        protected abstract void SetPredecessors( List<T> items );

        public List<T> ExecutionSequence { get; }

        protected void SetPredecessor<TWalker, TPred>( List<ISyntaxWalker> walkers )
            where TWalker : ISyntaxWalker
            where TPred : ISyntaxWalker
        {
            var walker = walkers.FirstOrDefault( w => w is TWalker );

            if( walker == null )
                throw new ArgumentException( $"Couldn't find {nameof(ISyntaxWalker)} '{typeof(TWalker)}'" );

            var predecessor = walkers.FirstOrDefault( w => w is TPred );

            if( predecessor == null )
                throw new ArgumentException( $"Couldn't find {nameof(ISyntaxWalker)} '{typeof(TPred)}'" );

            walker.Predecessor = predecessor;
        }
    }
}
