using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using J4JSoftware.Logging;
using J4JSoftware.Roslyn;

namespace J4JSoftware.Roslyn
{
    public class TopologicalActions<TAction, TArg> : IEnumerable<TAction>
        where TAction: class, IEquatable<TAction>, ITopologicalAction<TArg>
    {
        private readonly List<TAction> _items;

        private readonly TopologicallySortableCollection<TAction> _collection =
            new TopologicallySortableCollection<TAction>();

        protected TopologicalActions(
            IEnumerable<TAction> items,
            IJ4JLogger logger
        )
        {
            _items = items.ToList();

            Logger = logger;
            Logger.SetLoggedType(this.GetType());
        }

        protected IJ4JLogger Logger { get; }

        public int NumRoots => _collection.Edges( null ).Count;

        public bool Add<TItem>()
            where TItem : TAction
        {
            var itemType = typeof(TItem);

            var item = _items.FirstOrDefault(x => x.GetType() == itemType);

            if (item == null)
            {
                Logger.Error<Type>("No instance of type ({0}) exists in the available items", itemType);
                return false;
            }

            _collection.Add( item );

            return true;
        }

        public bool Add<TCurrent, TPredecessor>()
            where TCurrent : TAction
            where TPredecessor : TAction
        {
            var curType = typeof(TCurrent);
            var predType = typeof(TPredecessor);

            if( curType == predType )
            {
                Logger.Error("Current and predecessor types of are equal, which is not allowed"  );
                return false;
            }

            var current = _items.FirstOrDefault( x => x.GetType() == curType );
            if( current == null )
            {
                Logger.Error<Type>("No instance of current type ({0}) exists in the available items", curType  );
                return false;
            }

            var predecessor = _items.FirstOrDefault( x => x.GetType() == predType );
            if( predecessor == null )
            {
                Logger.Error<Type>( "No instance of end type ({0}) exists in the available items", predType );
                return false;
            }

            _collection.Add( current, predecessor );

            return true;
        }

        public void Clear() => _collection.Clear();

        public void Remove( TAction toRemove )
        {
            if( _items.Any( x => x == toRemove ) )
                _collection.Remove( toRemove );
        }

        public IEnumerator<TAction> GetEnumerator()
        {
            if( !_collection.Sort( out var sorted, out var remainingEdges ) )
            {
                Logger.Error("Failed to sort the items");
                yield break;
            }

            foreach( var item in sorted! )
            {
                yield return item;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}