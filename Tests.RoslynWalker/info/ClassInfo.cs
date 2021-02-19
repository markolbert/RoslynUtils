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

namespace Tests.RoslynWalker
{
    public class NamedTypeSource
    {
        public string Accessibility { get; init; }
        public string Ancestry {get; init; }
        public string Name { get; init; }
        public List<string> TypeArguments { get; init; }
    }

    public class ClassInfo : InterfaceInfo
    {
        public ClassInfo( NamedTypeSource src )
            : base( ElementNature.Class )
        {
            Name = src.Name;
            Ancestry = src.Ancestry;
            Accessibility = SourceRegex.ParseAccessibility( src.Accessibility, out var temp )
                ? temp!
                : Accessibility.Undefined;
            TypeArguments = src.TypeArguments;
        }

        public ClassInfo()
            : base( ElementNature.Class )
        {
        }

        public List<FieldInfo> Fields { get; } = new();
        public List<DelegateInfo> Delegates { get; } = new();

        //public new static ClassInfo Create( SourceLine srcLine )
        //{
        //    var retVal = new ClassInfo( srcLine.ElementName!, srcLine.Accessibility );

        //    retVal.TypeArguments.AddRange( GetNamedTypeTypeArguments( srcLine.Line ) );

        //    return retVal;
        //}
    }
}