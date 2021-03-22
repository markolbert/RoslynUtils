#region license

// Copyright 2021 Mark A. Olbert
// 
// This library or program 'DocCompilerDb' is free software: you can redistribute it
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

using System.Collections;
using System.Collections.Generic;

namespace J4JSoftware.DocCompiler
{
    public class NamedType : NamedTypeReference
    {
        protected NamedType()
        {
        }

        public int SourceBlockID { get; set; }
        public SourceBlock SourceBlock { get; set; }
        public ICollection<Method> Methods { get; set; }
        public ICollection<Property> Properties { get; set; }
        public ICollection<Event> Events { get; set; }
        public ICollection<TypeParameter> TypeParameters { get; set; }
        public ICollection<TypeArgument> TypeArguments { get; set; }
        public ICollection<TypeConstraint> TypeConstraints { get; set; }
        public OtherTypeConstraints OtherTypeConstraints { get; set; }
        public ICollection<TypeAncestor> Ancestors { get; set; }
    }
}