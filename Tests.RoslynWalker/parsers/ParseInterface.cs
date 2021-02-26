using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Tests.RoslynWalker
{
    public class ParseInterface : ParseInterfaceClassBase<InterfaceInfo>
    {
        public ParseInterface()
            : base( ElementNature.Interface, @"(.*\s+interface|^interface)\s+", ParserFocus.CurrentSourceLine, LineType.BlockOpener )
        {
        }

        protected override List<InterfaceInfo>? Parse( SourceLine srcLine )
        {
            var ntSource = ParseInternal( srcLine );
            if( ntSource == null )
                return null;

            var info = new InterfaceInfo( ntSource )
            {
                Parent = GetParent( srcLine, ElementNature.Namespace, ElementNature.Class )
            };

            return new List<InterfaceInfo> { info };
        }
    }
}