﻿using System.Collections.Generic;

namespace Tests.RoslynWalker
{
    public class ParseInterface : ParseInterfaceClassBase<InterfaceInfo>
    {
        public ParseInterface()
            : base( ElementNature.Interface, @"(.*\s+interface|^interface)\s+", LineType.BlockOpener )
        {
        }

        protected override List<InterfaceInfo>? Parse( StatementLine srcLine )
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