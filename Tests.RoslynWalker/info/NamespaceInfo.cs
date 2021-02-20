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

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Tests.RoslynWalker
{
    public class NamespaceInfo : BaseInfo
    {
        private readonly List<InterfaceInfo> _interfaces = new();
        private readonly List<ClassInfo> _classes = new();

        public NamespaceInfo( string name )
            : base( ElementNature.Namespace, name)
        {
        }

        public ReadOnlyCollection<InterfaceInfo> Interfaces => _interfaces.AsReadOnly();
        public ReadOnlyCollection<ClassInfo> Classes => _classes.AsReadOnly();

        public void AddInterface( InterfaceInfo toAdd )
        {
            var existing =
                _interfaces.FirstOrDefault( x => x.FullName.Equals( toAdd.FullName, StringComparison.Ordinal ) );

            if( existing == null )
                _interfaces.Add( toAdd );
            else
            {
                existing.Events.AddRange( toAdd.Events );
                existing.Methods.AddRange(toAdd.Methods);
                existing.Properties.AddRange( toAdd.Properties );
            }
        }

        public void AddClass( ClassInfo toAdd )
        {
            var existing =
                _classes.FirstOrDefault( x => x.FullName.Equals( toAdd.FullName, StringComparison.Ordinal ) );

            if( existing == null )
                _interfaces.Add( toAdd );
            else
            {
                existing.Events.AddRange( toAdd.Events );
                existing.Methods.AddRange(toAdd.Methods);
                existing.Properties.AddRange( toAdd.Properties );
                existing.Delegates.AddRange(toAdd.Delegates);
                existing.Fields.AddRange( toAdd.Fields );
            }
        }
    }
}