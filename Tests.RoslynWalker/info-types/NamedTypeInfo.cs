using System;
using System.Collections.Generic;

#pragma warning disable 8618

namespace Tests.RoslynWalker
{
    public class NamedTypeInfo : ICodeElement
    {
        protected NamedTypeInfo( string name, Accessibility accessibility )
        {
            Name = name;
            Accessibility = accessibility;
        }

        public string Name { get; }
        public Accessibility Accessibility { get; }
    }
}