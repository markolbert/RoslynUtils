using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Tests.RoslynWalker
{
    public class ParseEvent : ParseBase<EventInfo>
    {
        private static readonly Regex _rxEvent = new(@"\s*(.*)(?:event)\s+EventHandler<?(.*)>?\s+(.*)\s*");


        public ParseEvent()
            : base( ElementNature.Event, 
                @"(.*\s+event|^event)\s+", 
                new[] { LineType.BlockOpener, LineType.Statement } )
        {
        }

        protected override List<EventInfo>? Parse( StatementLine srcLine )
        {
            var match = _rxEvent.Match(srcLine.Line);

            if (!match.Success
                || match.Groups.Count != 4 )
                return null;

            // if the event arg type is generic we need to remove the
            // ending/extra '>'
            var argType = match.Groups[ 2 ].Value.Trim();
            if( argType.Length >= 4 && argType[ ^1 ] == '>' )
                argType = argType[ ..^1 ];

            var eventSrc = new EventSource(match.Groups[3].Value.Trim(),
                match.Groups[1].Value.Trim(),
                argType);

            var info= new EventInfo( eventSrc )
            {
                Parent = GetParent( srcLine, ElementNature.Class, ElementNature.Interface )
            };

            return new List<EventInfo> { info };
        }
    }
}