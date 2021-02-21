using System;
using System.Text.RegularExpressions;

namespace Tests.RoslynWalker
{
    public class ParseEvent : ParseBase<EventInfo>
    {
        private static readonly Regex RxEvent = new(@"\s*(.*)(?:event)\s+(.*)\s+(.*)\s*");


        public ParseEvent()
            : base( ElementNature.Event, @"\s*event\s+")
        {
        }

        public override EventInfo? Parse( SourceLine srcLine )
        {
            var toProcess = GetSourceLineToProcess( srcLine );

            var match = RxEvent.Match(toProcess.Line);

            if (!match.Success
                || match.Groups.Count != 4
                || !ExtractTypeArguments(match.Groups[2].Value.Trim(), out var baseType, out var typeArgs))
                return null;

            var eventSrc = new EventSource(match.Groups[3].Value.Trim(),
                match.Groups[1].Value.Replace("event", string.Empty).Trim(),
                baseType!,
                typeArgs);

            return new EventInfo( eventSrc )
            {
                Parent = GetParent( toProcess, ElementNature.Class, ElementNature.Interface )
            };
        }
    }
}