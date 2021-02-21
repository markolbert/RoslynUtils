using System;
using System.Collections;
using System.Text.RegularExpressions;

namespace Tests.RoslynWalker
{
    public class ParseInterface : ParseBase<InterfaceInfo>
    {
        protected static readonly Regex RxAncestry = new(@"\s*([^:]+):?\s*(.*)", RegexOptions.Compiled);
        protected static readonly Regex RxNamedType = new(@$"\s*({AccessibilityClause})?\s*(class|interface)?\s*(.*)");

        public ParseInterface()
            : base( ElementNature.Interface, @"\s*interface\s+" )
        {
        }
        protected ParseInterface( ElementNature nature, string matchText )
            : base( nature, matchText )
        {
        }

        public override InterfaceInfo? Parse( SourceLine srcLine )
        {
            var toProcess = GetSourceLineToProcess( srcLine );

            return !ExtractNamedTypeArguments( toProcess.Line, "interface", out var ntSource )
                ? null
                : new InterfaceInfo( ntSource! )
                {
                    Parent = GetParent( toProcess, ElementNature.Namespace, ElementNature.Class )
                };
        }

        protected bool ExtractNamedTypeArguments(string text, string nature, out NamedTypeSource? result)
        {
            result = null;

            if (!ExtractAncestry(text, out var fullDecl, out var ancestry))
                return false;

            if (!ExtractTypeArguments(fullDecl!, out var baseDecl, out var typeArgs))
                return false;

            var match = RxNamedType.Match(text);

            if (!match.Success
                || match.Groups.Count != 4
                || !match.Groups[2].Value.Trim().Equals(nature, StringComparison.Ordinal))
                return false;

            result = new NamedTypeSource(match.Groups[3].Value.Trim(),
                match.Groups[1].Value.Trim().Replace(" ", ""),
                ancestry!,
                typeArgs);

            return true;
        }

        protected bool ExtractAncestry(string text, out string? preamble, out string? ancestry)
        {
            preamble = null;
            ancestry = null;

            var match = RxAncestry.Match(text);

            if (!match.Success)
                return false;

            if (match.Groups.Count != 3)
                return false;

            ancestry = match.Groups[2].Value.Trim();
            preamble = match.Groups[1].Value.Trim();

            return true;
        }
    }
}