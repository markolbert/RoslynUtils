using System;
using System.Collections.Generic;
using System.Linq;
using J4JSoftware.Logging;

namespace J4JSoftware.Roslyn
{
    public abstract class TopologicallySortedCollection<T, TRoot> : ITopologicallySorted<T>
        where T : class, ITopologicalSort<T>
        where TRoot : T
    {
        protected readonly List<T> _rawItems;
        private readonly List<T> _items = new List<T>();

        protected TopologicallySortedCollection(
            IEnumerable<T> items,
            IJ4JLogger logger
        )
        {
            Logger = logger;
            Logger.SetLoggedType( this.GetType() );

            _rawItems = items.ToList();

            var rootType = typeof(TRoot);
            var rootNode = _rawItems.FirstOrDefault(x => x.GetType() == rootType);

            if (rootNode == null)
                Logger.Error<Type>("Couldn't find '{rootType}'", rootType);
            else
                _items.Add( rootNode );

            SetPredecessors();

            switch( _items.Count(x=>x.Predecessor == null ) )
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

            if( TopologicalSorter.Sort( _items, out var result ) )
                ExecutionSequence = result!;
            else
            {
                Logger.Error<Type>( "Couldn't create execution sequence for {0}", typeof(T) );
                ExecutionSequence = new List<T>();
            }
        }

        protected IJ4JLogger Logger { get; }

        protected abstract void SetPredecessors();

        public List<T> ExecutionSequence { get; }

        protected bool SetPredecessor<TNode, TPredecessorNode>()
            where TNode : T
            where TPredecessorNode : T
        {
            var nodeType = typeof(TNode);
            var predecessorType = typeof(TPredecessorNode);

            var node = _rawItems.FirstOrDefault( x => x.GetType() == nodeType );

            if( node == null )
            {
                Logger.Error<Type>( "Couldn't find '{nodeType}'", nodeType );
                return false;
            }

            var predecessor = _rawItems.FirstOrDefault( x => x.GetType() == predecessorType );

            if( predecessor == null )
            {
                Logger.Error<Type>($"Couldn't find '{predecessorType}'", predecessorType);
                return false;
            }

            node.Predecessor = predecessor;

            _items.Add( node );

            return true;
        }
    }
}
