using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using J4JSoftware.Logging;
using J4JSoftware.Roslyn;

namespace Tests.RoslynWalker
{
    public class TopologicalCollection<TAction, TArg> : IEnumerable<TAction>
        where TAction: class, IEquatable<TAction>, ITopologicalAction<TArg>
    {
        private readonly List<TAction> _items;

        private readonly TopologicallySortableCollection<TAction> _collection =
            new TopologicallySortableCollection<TAction>();

        protected TopologicalCollection(
            IEnumerable<TAction> items,
            IJ4JLogger logger
        )
        {
            _items = items.ToList();

            Logger = logger;
            Logger.SetLoggedType(this.GetType());
        }

        protected IJ4JLogger Logger { get; }

        protected bool Add<TStart, TEnd>()
            where TStart : TAction
            where TEnd : TAction
        {
            var startType = typeof(TStart);
            var endType = typeof(TEnd);

            if( startType == endType )
            {
                Logger.Error("Start and end types of are equal, which is not allowed"  );
                return false;
            }

            var start = _items.FirstOrDefault( x => x.GetType() == startType );
            if( start == null )
            {
                Logger.Error<Type>("No instance of start type ({0}) exists in the available items", startType  );
                return false;
            }

            var end = _items.FirstOrDefault( x => x.GetType() == endType );
            if( end == null )
            {
                Logger.Error<Type>( "No instance of end type ({0}) exists in the available items", endType );
                return false;
            }

            _collection.Add( start, end );

            return true;
        }

        protected void Clear() => _collection.Clear();

        protected void Remove( TAction toRemove )
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