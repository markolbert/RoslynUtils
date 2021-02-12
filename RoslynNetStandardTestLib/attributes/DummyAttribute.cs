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

// ReSharper disable ValueParameterNotUsed
#pragma warning disable 67
#pragma warning disable 8618

namespace J4JSoftware.Roslyn.Tests
{
    [ AttributeUsage( AttributeTargets.Class | AttributeTargets.Interface, Inherited = false, AllowMultiple = true ) ]
    public class DummyAttribute : Attribute
    {
        public int TestField;

        public DummyAttribute( string arg1, Type arg2 )
        {
        }

        public event EventHandler<int> PlainEvent;

        public event EventHandler<int> CustomAddRemoveEvent
        {
            add => PlainEvent += OnCustomEventAdd;
            remove => PlainEvent -= OnCustomEventRemove;
        }

        private void OnCustomEventAdd( object? sender, int e )
        {
            throw new NotImplementedException();
        }

        private void OnCustomEventRemove( object? sender, int e )
        {
            throw new NotImplementedException();
        }
    }
}