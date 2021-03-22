#region license

// Copyright 2021 Mark A. Olbert
// 
// This library or program 'RoslynNetStandardTestLib' is free software: you can redistribute it
// and/or modify it under the terms of the GNU General Public License as
// published by the Free Software Foundation, either version 3 of the License,
// or (at your option) any later version.
// 
// This library or program is distributed in the hope that it will be useful, but
// WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// General Public License for more details.
// 
// You should have received a copy of the GNU General Public License along with
// this library or program.  If not, see <https://www.gnu.org/licenses/>.

#endregion

using System;
using System.Collections.Generic;
using System.Linq;

#pragma warning disable 8618

namespace J4JSoftware.Roslyn.Tests
{
    public class LinqClass
    {
        private readonly List<string> _text = new();

        public int IntegerProperty { get; protected set; }

        public int this[ string key ] => -1;

        public SimpleGeneric<int, int> GenericProperty { get; protected set; }

        public int this[ SimpleGeneric<int, int> key ] => -1;

        public List<string> GetFiltered( string text )
        {
            return _text
                .Where( t => t.IndexOf( text, StringComparison.OrdinalIgnoreCase ) >= 0 )
                .ToList();
        }
    }
}