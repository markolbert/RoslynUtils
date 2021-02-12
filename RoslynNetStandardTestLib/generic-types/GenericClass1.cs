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
    public class GenericClass1<T1, T2>
        where T1 : IEnumerable<T1>
        where T2 : new()
    {
        private T1 generic_field1;
        private T2 generic_field2;

        public T1 TOne { get; set; }
        public T2 TTwo { get; set; }
    }

    public class GenericClass1<T>
    {
        public T Property { get; set; }
        public T[] OneDimensionalArray { get; set; }
        public T[,] TwoDimensionalArray { get; set; }
    }

    public class DefinedGenericClass1 : GenericClass1<int>
    {
    }

    public class ComplexGeneric<T>
        where T : IEnumerable<T>
    {
        public TMethod SomeMethod<TMethod>( TMethod item )
            where TMethod : GenericClass1<T, int>
        {
            throw new NotImplementedException();
        }
    }
}