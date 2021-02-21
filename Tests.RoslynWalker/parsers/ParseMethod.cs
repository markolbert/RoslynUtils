using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Tests.RoslynWalker
{
    public class ParseMethod : ParseBase<MethodInfo>
    {
        protected static readonly Regex RxMethodArgsGroup = new(@$"\s*([^()]*)\(\s*(.*)\)");
        protected static readonly Regex RxMethodGroup = new(
            @$"\s*({AccessibilityClause})?\s*([^\s]*)\s*([^\s]*)",
            RegexOptions.Compiled);

        public ParseMethod()
            : base( ElementNature.Method, @".*\(.*\)" )
        {
        }

        public override MethodInfo? Parse( SourceLine srcLine )
        {
            var toProcess = GetSourceLineToProcess( srcLine );

            if (!ExtractMethodArguments(toProcess.Line, out var fullDecl, out var arguments))
                return null;

            if (!ExtractTypeArguments(fullDecl!, out var typeName, out var typeArguments))
                return null;

            if (!ExtractMethodElements(typeName!, out var methodSrc))
                return null;

            return new MethodInfo( methodSrc!, typeArguments!, arguments! )
            {
                Parent = GetParent( toProcess, ElementNature.Class, ElementNature.Interface )
            };
        }

        protected bool ExtractMethodArguments(string text, out string? preamble, out List<string> arguments)
        {
            preamble = null;
            arguments = new List<string>();

            var groupMatch = RxMethodArgsGroup.Match(text);

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

            var match = RxMethodGroup.Match(text);

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