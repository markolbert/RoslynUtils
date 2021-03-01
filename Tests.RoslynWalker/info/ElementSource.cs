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
    public record ElementSource( string Name, string Accessibility );

    public record DelegateSource(
            string Name,
            string Accessibility,
            List<string> TypeArguments,
            List<string> Arguments )
        : NamedTypeSource( Name, Accessibility, TypeArguments );

    public record EventSource(
            string Name,
            string Accessibility,
            string EventArgType )
        : ElementSource( Name, Accessibility );

    public record FieldSource(
            string Name,
            string Accessibility,
            string FieldType,
            string AssignmentClause )
        : ElementSource( Name, Accessibility );

    public record InterfaceSource(
            string Name,
            string Accessibility,
            List<string> TypeArguments,
            string Ancestry )
        : NamedTypeSource( Name, Accessibility, TypeArguments );

    public record NamedTypeSource(
            string Name,
            string Accessibility,
            List<string> TypeArguments )
        : ElementSource( Name, Accessibility );

    public record MethodSource(
            string Name,
            string Accessibility,
            List<string> TypeArguments,
            List<string> Arguments,
            string ReturnType )
        : DelegateSource( Name, Accessibility, TypeArguments, Arguments );
}