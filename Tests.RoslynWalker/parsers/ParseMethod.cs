using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Tests.RoslynWalker
{
    public class ParseMethod : ParseBase<MethodInfo>
    {
        private static readonly Regex _rxMethodArgsGroup = new(@$"\s*([^()]*)\(\s*(.*)\)");
        private static readonly Regex _rxMethodGroup = new(
            @$"\s*({AccessibilityClause})?\s*([^\s]*)\s*([^\s]*)",
            RegexOptions.Compiled);

        public ParseMethod()
            : base( ElementNature.Method, @".*\(.*\)", ParserFocus.CurrentSourceLine, LineType.Statement )
        {
        }

        protected override MethodInfo? Parse( SourceLine srcLine )
        {
            if (!ExtractMethodArguments(srcLine.Line, out var fullDecl, out var arguments))
                return null;

            if (!ExtractTypeArguments(fullDecl!, out var typeName, out var typeArguments))
                return null;

            if (!ExtractMethodElements(typeName!, out var methodSrc))
                return null;

            return new MethodInfo( methodSrc!, typeArguments!, arguments! )
            {
                Parent = GetParent( srcLine, ElementNature.Class, ElementNature.Interface )
            };
        }

        protected bool ExtractMethodArguments(string text, out string? preamble, out List<string> arguments)
        {
            preamble = null;
            arguments = new List<string>();

            var groupMatch = _rxMethodArgsGroup.Match(text);

            if (!groupMatch.Success
                || groupMatch.Groups.Count != 3)
                return false;

            preamble = groupMatch.Groups[1].Value.Trim();

            var remainder = groupMatch.Groups[2].Value.Trim();

            // if no arguments we're done
            if (string.IsNullOrEmpty(remainder))
                return true;

            arguments.AddRange(ParseArguments(remainder, true));

            return true;
        }

        protected bool ExtractMethodElements( string text, out ReturnTypeSource? result)
        {
            result = null;

            var match = _rxMethodGroup.Match(text);

            if (!match.Success
                || match.Groups.Count != 4)
                return false;

            result = new ReturnTypeSource(
                match.Groups[3].Value.Trim(),
                match.Groups[1].Value.Trim(),
                match.Groups[2].Value.Trim());

            return true;
        }
    }
}