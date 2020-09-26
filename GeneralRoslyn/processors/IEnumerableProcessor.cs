using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn
{
    public interface IEnumerableProcessor<TItem> : ITopologicalAction<TItem>, IEquatable<IEnumerableProcessor<TItem>>
    {
    }

    public interface IProcessorCollection<in TItem>
    {
        bool Process( IEnumerable<TItem> items );
    }


}