using System;
using System.Collections.Generic;
using System.Linq;

#pragma warning disable 8618

namespace Tests.RoslynWalker
{
    public class PropertyInfo : ICodeElement
    {
        public static PropertyInfo Create( SourceLine srcLine)
        {
            var nameParts = srcLine.Line
                .Split( " ", StringSplitOptions.RemoveEmptyEntries );

            return new PropertyInfo( nameParts.Last(), srcLine.Accessibility );
        }

        private PropertyInfo( string name, Accessibility accessibility )
        {
            Name = name;
            Accessibility = accessibility;
        }

        public string Name { get; }
        public Accessibility Accessibility { get; }
    }
}