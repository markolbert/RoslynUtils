using System;
using System.Collections.Generic;
using System.Linq;

#pragma warning disable 8618

namespace Tests.RoslynWalker
{
    public class EventInfo : ICodeElement
    {
        public static EventInfo Create( SourceLine srcLine)
        {
            var nameParts = srcLine.Line
                .Split( " ", StringSplitOptions.RemoveEmptyEntries );

            return new EventInfo( nameParts.Last(), srcLine.Accessibility );
        }

        private EventInfo( string name, Accessibility accessibility )
        {
            Name = name;
            Accessibility = accessibility;
        }

        public string Name { get; }
        public Accessibility Accessibility {get;}
    }
}