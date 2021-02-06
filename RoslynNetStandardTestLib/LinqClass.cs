using System;
using System.Collections.Generic;
using System.Linq;
#pragma warning disable 8618

namespace J4JSoftware.Roslyn.Tests
{
    public class LinqClass
    {
        private readonly List<string> _text = new List<string>();

        public List<string> GetFiltered( string text ) => _text
            .Where( t => t.IndexOf( text, StringComparison.OrdinalIgnoreCase ) >= 0 )
            .ToList();

        public int IntegerProperty { get; protected set; }

        public int this[ string key ]
        {
            get => -1;
        }

        public SimpleGeneric<int, int> GenericProperty { get; protected set; }

        public int this[ SimpleGeneric<int, int> key ]
        {
            get => -1;
        }
    }
}