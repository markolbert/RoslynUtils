using System;
using System.Collections.Generic;
using System.Linq;

namespace J4JSoftware.Roslyn.Tests
{
    public class LinqClass
    {
        private readonly List<string> _text = new List<string>();

        public List<string> GetFiltered( string text ) => _text
            .Where( t => t.IndexOf( text, StringComparison.OrdinalIgnoreCase ) >= 0 )
            .ToList();
    }
}
