using System;
using System.Collections.Generic;
using System.Linq;

#pragma warning disable 8618

namespace Tests.RoslynWalker
{
    public class FieldInfo : ICodeElement
    {
        public static FieldInfo Create( SourceLine srcLine)
        {
            var nameParts = srcLine.Line
                .Split( " ", StringSplitOptions.RemoveEmptyEntries );

            return new FieldInfo( nameParts.Last(), srcLine.Accessibility );
        }

        private FieldInfo( string name, Accessibility accessibility )
        {
            Name = name;
            Accessibility = accessibility;
        }

        public string Name { get; }
        public Accessibility Accessibility {get;}
    }
}