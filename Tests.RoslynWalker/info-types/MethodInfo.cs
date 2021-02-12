using System;
using System.Collections.Generic;

#pragma warning disable 8618

namespace Tests.RoslynWalker
{
    public class MethodInfo : ICodeElement, ICodeElementTypeArguments
    {
        public static MethodInfo Create( SourceLine srcLine)
        {
            var openParenLoc = srcLine.Line.IndexOf( "(", StringComparison.Ordinal );
            var lesserThanLoc = srcLine.Line.IndexOf( "<", StringComparison.Ordinal );
            var closeParenLoc = srcLine.Line.IndexOf( ")", StringComparison.Ordinal );

            var nameParts = srcLine.Line[..(lesserThanLoc< 0 ? openParenLoc : lesserThanLoc)]
                .Split( " ", StringSplitOptions.RemoveEmptyEntries );

            var retVal = new MethodInfo( nameParts.Length > 1 ? nameParts[ 1 ] : nameParts[ 0 ],
                srcLine.Accessibility );

            if( lesserThanLoc >= 0)
                retVal.TypeArguments.AddRange( SourceText.GetTypeArgs( srcLine.Line[ lesserThanLoc.. ] ) );

            retVal.Arguments.AddRange( SourceText.GetArgs( srcLine.Line[ openParenLoc.. ] ) );

            return retVal;
        }

        public MethodInfo( string name, Accessibility accessibility )
        {
            Name = name;
            Accessibility = accessibility;
        }

        public string Name { get; }
        public Accessibility Accessibility {get;}
        public List<string> Arguments { get; } = new();
        public List<string> TypeArguments { get; } = new();
    }
}