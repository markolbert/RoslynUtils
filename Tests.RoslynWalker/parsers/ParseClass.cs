using System.Collections.Generic;

namespace Tests.RoslynWalker
{
    public class ParseClass : ParseInterfaceClassBase<ClassInfo>
    {
        public ParseClass()
            : base( ElementNature.Class, @"(.*\s+class|^class)\s+", LineType.BlockOpener )
        {
        }

        protected override List<ClassInfo>? Parse( StatementLine srcLine )
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