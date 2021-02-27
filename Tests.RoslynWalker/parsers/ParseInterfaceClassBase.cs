using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Tests.RoslynWalker
{
    public abstract class ParseInterfaceClassBase<TInfo> : ParseBase<TInfo>
    where TInfo : InterfaceInfo
    {
        private static readonly Regex _rxAncestry = new(@"\s*([^:]+):?\s*(.*)", RegexOptions.Compiled);
        private static readonly Regex _rxNamedType = new(@$"\s*({AccessibilityClause})?\s*(class|interface)?\s*(.*)");

        protected ParseInterfaceClassBase( ElementNature nature, string matchText, LineType lineType )
            : base( nature, matchText, lineType )
        {
        }

        protected NamedTypeSource? ParseInternal( SourceLine srcLine ) =>
            ExtractNamedTypeArguments( srcLine.Line, typeof(TInfo) == typeof(ClassInfo) ? "class" : "interface" );

        private NamedTypeSource? ExtractNamedTypeArguments(string text, string nature)
        {
            if (!ExtractAncestry(text, out var fullDecl, out var ancestry))
                return null;

            if (!ExtractTypeArguments(fullDecl!, out var baseDecl, out var typeArgs))
                return null;

            var match = _rxNamedType.Match(baseDecl!);

            if (!match.Success
                || match.Groups.Count != 4
                || !match.Groups[2].Value.Trim().Equals(nature, StringComparison.Ordinal))
                return null;

            return new NamedTypeSource(match.Groups[3].Value.Trim(),
                match.Groups[1].Value.Trim().Replace(" ", ""),
                ancestry!,
                typeArgs);
        }

        private bool ExtractAncestry(string text, out string? preamble, out string? ancestry)
        {
            preamble = null;
            ancestry = null;

            var match = _rxAncestry.Match(text);

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