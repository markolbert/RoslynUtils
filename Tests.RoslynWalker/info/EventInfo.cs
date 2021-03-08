#region license

// Copyright 2021 Mark A. Olbert
// 
// This library or program 'Tests.RoslynWalker' is free software: you can redistribute it
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

using System.Collections.Generic;
using System.Linq;

#pragma warning disable 8618

namespace Tests.RoslynWalker
{
    public class EventInfo : ElementInfo, IAttributable
    {
        public EventInfo( EventSource src )
            : base( ElementNature.Event, src )
        {
            EventArgType= src.EventArgType;

            Attributes = src.Attributes
                .Select( x => new AttributeInfo( x ) )
                .ToList();
        }

        public string EventArgType { get; }
        public List<AttributeInfo> Attributes { get; }
    }
}