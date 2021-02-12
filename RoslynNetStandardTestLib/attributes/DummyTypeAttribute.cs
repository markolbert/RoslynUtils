﻿#region license

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

namespace J4JSoftware.Roslyn.Tests
{
    [ AttributeUsage( AttributeTargets.Class, AllowMultiple = true ) ]
    public class DummyTypeAttribute : Attribute
    {
        public DummyTypeAttribute( Type dummyType )
        {
            DummyType = dummyType;
        }

        public Type DummyType { get; }
    }
}