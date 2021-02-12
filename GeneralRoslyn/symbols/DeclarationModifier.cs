#region license

// Copyright 2021 Mark A. Olbert
// 
// This library or program 'GeneralRoslyn' is free software: you can redistribute it
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

namespace J4JSoftware.Roslyn
{
    [ Flags ]
    public enum DeclarationModifier
    {
        Abstract = 1 << 0,
        Async = 1 << 1,
        Const = 1 << 2,
        New = 1 << 3,
        Override = 1 << 4,
        Partial = 1 << 5,
        ReadOnly = 1 << 6,
        Ref = 1 << 7,
        Sealed = 1 << 8,
        Static = 1 << 9,
        Unsafe = 1 << 10,
        Virtual = 1 << 11,
        WithEvents = 1 << 12,
        WriteOnly = 1 << 13,

        None = 0
    }
}