using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Tests.RoslynWalker
{
    public class ParseClass : ParseInterfaceClassBase<ClassInfo>
    {
        public ParseClass()
            : base( ElementNature.Class, @"(.*\s+class|^class)\s+", ParserFocus.CurrentSourceLine,
                LineType.BlockOpener )
        {
        }

        protected override List<ClassInfo>? Parse( SourceLine srcLine )
        {
            var ntSource = ParseInternal( srcLine );
            if( ntSource == null )
                return null;

            var info = new ClassInfo( ntSource )
            {
                Parent = GetParent( srcLine, ElementNature.Namespace, ElementNature.Class )
            };

            return new List<ClassInfo> { info };
        }
    }
}