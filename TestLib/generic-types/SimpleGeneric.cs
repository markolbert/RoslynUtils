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

#pragma warning disable 169
#pragma warning disable 8618

namespace J4JSoftware.Roslyn.Tests
{
    public class SimpleGeneric<T1, T2>
        where T1 : struct
        where T2 : struct
    {
        private Dictionary<T1, T2> generic_field1;
        private Dictionary<T2, T1> generic_field2;

        public T1 PropertyT1 { get; set; }
        public T2 PropertyT2 { get; set; }
        public T1[] ArrayPropertyT1 { get; set; }
        public T2[] ArrayPropertyT2 { get; set; }

        public TMethod[] GetArrayPropertyT3<TMethod>()
            where TMethod : new()
        {
            return Array.Empty<TMethod>();
        }
    }
}