using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
// ReSharper disable StaticMemberInGenericType

namespace Tests.RoslynWalker
{
    public abstract class ParseBase<TElement> : IParse
        where TElement : BaseInfo
    {
        protected const string AccessibilityClause = @"private\s+|public\s+|protected internal\s+|protected\s+|internal\s+";
        private static readonly Regex _rxTypeArgsGroup = new(@$"\s*([^<>]*)<(.*)>", RegexOptions.Compiled);

        private readonly Regex _matcher;
        private readonly List<LineType> _lineTypes;

        protected ParseBase( 
            ElementNature nature, 
            string matchText,
            LineType[] lineTypes
            )
        {
            _matcher = new Regex(matchText, RegexOptions.Compiled);
            _lineTypes = lineTypes.ToList();

            MatchText = matchText;
        }

        protected ParseBase( 
            ElementNature nature, 
            string matchText,
            LineType lineType
        )
        {
            _matcher = new Regex(matchText, RegexOptions.Compiled);
            _lineTypes = new List<LineType> { lineType };

            MatchText = matchText;
        }

        protected abstract List<TElement>? Parse( SourceLine srcLine );

        public string MatchText { get; }
        public ReadOnlyCollection<LineType> SupportedLineTypes => _lineTypes.AsReadOnly();

        public virtual bool HandlesLine( SourceLine srcLine ) => SupportedLineTypes.Any( x => x == srcLine.LineType )
                                                                 && _matcher.IsMatch( srcLine.Line );

        protected BaseInfo? GetParent(SourceLine srcLine, params ElementNature[] nature)
        {
            var curSrcLine = srcLine;
            BaseInfo? retVal = null;

            while (curSrcLine.Parent != null)
            {
                curSrcLine = curSrcLine.Parent.ParentLine;

                if (curSrcLine == null)
                    break;

                if (nature.All(x => curSrcLine?.Elements?.FirstOrDefault()?.Nature != x))
                    continue;

                retVal = curSrcLine.Elements!.First();
                break;
            }

            return retVal;
        }

        protected bool ExtractTypeArguments(string text, out string? preamble, out List<string> typeArgs)
        {
            preamble = null;
            typeArgs = new List<string>();

            var groupMatch = _rxTypeArgsGroup.Match(text);

            // if no type arguments return the entire text because it's just preamble
            if (!groupMatch.Success)
            {
                preamble = text.Trim();
                return true;
            }

            if (groupMatch.Groups.Count != 3)
                return false;

            preamble = groupMatch.Groups[1].Value.Trim();

            typeArgs.AddRange(ParseArguments(groupMatch.Groups[2].Value.Trim()));

            return true;
        }

        protected List<string> ParseArguments(string? text)
        {
            var retVal = new List<string>();

            if( string.IsNullOrEmpty( text ) )
                return retVal;
            
            var numLessThan = 0;
            var sb = new StringBuilder();

            foreach (var curChar in text)
            {
                switch (curChar)
                {
                    case ',':
                        if (numLessThan == 0)
                        {
                            retVal.Add(sb.ToString());
                            sb.Clear();
                        }
                        else sb.Append(curChar);

                        break;

                    case '<':
                        sb.Append(curChar);
                        numLessThan++;

                        break;

                    case '>':
                        sb.Append(curChar);
                        numLessThan--;

                        break;

                    default:
                        sb.Append(curChar);

                        break;
                }
            }

            if (sb.Length > 0)
                retVal.Add(sb.ToString());

            return retVal;
        }

        protected bool ParseAccessibility(string toParse, out Accessibility result)
        {
            if (string.IsNullOrEmpty(toParse))
            {
                result = Accessibility.Private;
                return true;
            }

            result = Accessibility.Undefined;

            if (!Enum.TryParse(typeof(Accessibility),
                toParse.Replace(" ", string.Empty),
                true,
                out var parsed))
                return false;

            result = (Accessibility)parsed!;

            return true;
        }

        List<BaseInfo>? IParse.Parse( SourceLine srcLine ) => !HandlesLine( srcLine ) ? null : Parse( srcLine )?.Cast<BaseInfo>().ToList();
    }
}