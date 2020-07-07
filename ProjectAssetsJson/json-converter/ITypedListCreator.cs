using System;
using System.Collections;

namespace J4JSoftware.Roslyn
{
    public interface ITypedListCreator
    {
        void Clear();
        void Add<TItem>( TItem value );
        Type GetListType();
        IList GetList();
    }
}