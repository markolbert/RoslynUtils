using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Tests.RoslynWalker
{
    public class ParseProperty : ParseBase<PropertyInfo>
    {
        protected static readonly Regex RxMethodArgsGroup = new(@$"\s*([^()]*)\(\s*(.*)\)");
        protected static readonly Regex RxMethodGroup = new(
            @$"\s*({AccessibilityClause})?\s*([^\s]*)\s*([^\s]*)",
            RegexOptions.Compiled);

        public ParseProperty()
            : base( ElementNature.Property, @".*get\s*|set\s*" )
        {
        }

        public override PropertyInfo? Parse( SourceLine srcLine )
        {
            var toProcess = GetSourceLineToProcess( srcLine );

        }
    }
}