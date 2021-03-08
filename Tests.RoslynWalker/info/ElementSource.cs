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
    public record BaseSource( string Name );

    public record ElementSource( string Name, string Accessibility )
        : BaseSource( Name );

    public record AttributeArgumentSource( string Name, string AssignmentClause )
        : BaseSource( Name );

    public record AttributeSource( string Name, List<AttributeArgumentSource> Arguments )
        : BaseSource( Name );

    public record ArgumentSource( string Name, string Type, List<string> attributeClauses )
        : BaseSource( Name );

    public record DelegateSource(
            string Name,
            string Accessibility,
            string ReturnType,
            List<string> TypeArguments,
            List<ArgumentSource> Arguments,
            List<AttributeSource> Attributes )
        : NamedTypeSource( Name, Accessibility, TypeArguments, Attributes );

    public record EventSource(
            string Name,
            string Accessibility,
            string EventArgType,
            List<AttributeSource> Attributes )
        : ElementSource( Name, Accessibility );

    public record FieldSource(
            string Name,
            string Accessibility,
            string FieldType,
            string AssignmentClause,
            List<AttributeSource> Attributes )
        : ElementSource( Name, Accessibility );

    public record InterfaceSource(
            string Name,
            string Accessibility,
            List<string> TypeArguments,
            List<AttributeSource> Attributes,
            string Ancestry )
        : NamedTypeSource( Name, Accessibility, TypeArguments, Attributes );

    public record NamedTypeSource(
            string Name,
            string Accessibility,
            List<string> TypeArguments,
            List<AttributeSource> Attributes )
        : ElementSource( Name, Accessibility );

    public record MethodSource(
            string Name,
            string Accessibility,
            List<string> TypeArguments,
            List<ArgumentSource> Arguments,
            string ReturnType,
            List<AttributeSource> Attributes )
        : DelegateSource( Name, Accessibility, ReturnType, TypeArguments, Arguments, Attributes );
}