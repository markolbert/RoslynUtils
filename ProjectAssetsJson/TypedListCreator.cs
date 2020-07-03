using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using J4JSoftware.Logging;

namespace J4JSoftware.Roslyn
{
    public class TypedListCreator : ITypedListCreator
    {
        private readonly IJ4JLogger _logger;
        private readonly List<object> _items = new List<object>();
        private readonly List<Type> _itemTypes = new List<Type>();

        public TypedListCreator( IJ4JLogger logger )
        {
            _logger = logger;
            _logger.SetLoggedType( this.GetType() );
        }

        public void Clear()
        {
            _items.Clear();
            _itemTypes.Clear();
        }

        public void Add<TItem>( TItem value )
        {
            switch( value )
            {
                case bool boolVal:
                    _items.Add( boolVal );
                    AddItemType<bool>();
                    break;

                case string textVal:
                    _items.Add( textVal );
                    AddItemType<string>();
                    break;

                case int intVal:
                    _items.Add( intVal );
                    AddItemType<int>();
                    break;

                case double dblVal:
                    _items.Add( dblVal );
                    AddItemType<double>();
                    break;

                case decimal decVal:
                    _items.Add( decVal );
                    AddItemType<decimal>();
                    break;

                case ExpandoObject eoVal:
                    _items.Add( eoVal );
                    AddItemType<ExpandoObject>();
                    break;

                default:
                    _items.Add( value! );
                    AddItemType<object>();
                    break;
            }
        }

        public Type GetListType()
        {
            if( _itemTypes.Count == 1 )
                return _itemTypes[ 0 ];

            return typeof(object);
        }

        public IList GetList()
        {
            var listType = typeof(List<>);
            var genListType = listType.MakeGenericType( GetListType() );

            var retVal = Activator.CreateInstance( genListType ) as IList;

            _items.ForEach(x=>retVal!.Add(x));

            return retVal!;
        }

        private void AddItemType<TItem>()
        {
            if( !_itemTypes.Contains( typeof( TItem ) ) )
                _itemTypes.Add( typeof( TItem ) );
        }
    }
}