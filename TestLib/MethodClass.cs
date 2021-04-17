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
using System.Collections;
using System.Collections.Generic;

namespace J4JSoftware.Roslyn.Tests
{
    public class MethodClass<T>
    {
        public void SomeMethod1()
        {
        }

        public int SomeMethod2()
        {
            return 0;
        }

        public (int IntProp, bool BoolProp) SomeMethod3()
        {
            return ( 0, true );
        }

        public (int IntProp, bool BoolProp) SomeMethod3( ref (int arg1, string arg2) tupleArg )
        {
            return ( 0, true );
        }

        public void SomeMethod4( int arg1, string[] arg2, List<bool> arg3 )
        {
        }

        public T1 SomeMethod5<T1>( T arg1, out string? result )
            where T1 : class, new()
        {
            throw new NotImplementedException();
        }

        public T1 SomeMethod6<T1>( T1 arg1, ref string[] result )
            where T1 : class, new()
        {
            throw new NotImplementedException();
        }

        public T1 SomeMethod6<T1>( T1 arg1, string[] result )
            where T1 : class, new()
        {
            throw new NotImplementedException();
        }

        public T1 SomeMethod7<T1>( T1 arg1, ref List<string> result )
            where T1 : class, new()
        {
            throw new NotImplementedException();
        }

        public T1 SomeMethod7<T1, T2>( T2 arg1, ref List<string> result )
            where T1 : class, new()
        {
            throw new NotImplementedException();
        }

        public SimpleGeneric<T1, T2> SomeMethod7<T1, T2>( SimpleGeneric<T1, T2> arg1, ref List<string> result )
            where T1 : struct
            where T2 : struct
        {
            throw new NotImplementedException();
        }
    }
}