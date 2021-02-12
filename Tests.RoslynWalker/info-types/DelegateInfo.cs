using System;
using System.Collections.Generic;

namespace Tests.RoslynWalker
{
    public class DelegateInfo : NamedTypeInfo
    {
        public static DelegateInfo Create( SourceLine srcLine )
        {
            var parts = srcLine.Line.Split( " ", StringSplitOptions.RemoveEmptyEntries );
            var text = parts.Length > 3? parts[ 3 ][..^1] : parts[2];

            var openParenLoc = text.IndexOf( "(", StringComparison.Ordinal );

            var retVal = new DelegateInfo( text[ ..openParenLoc ], srcLine.Accessibility );

            retVal.DelegateArguments.AddRange( SourceText.GetArgs( text ) );

            return retVal;
        }

        public DelegateInfo( string name, Accessibility accessibility )
            :base( name, accessibility )
        {
        }

        public List<string> TypeArguments { get; } = new();
        public List<string> DelegateArguments { get; } = new();
    }
}