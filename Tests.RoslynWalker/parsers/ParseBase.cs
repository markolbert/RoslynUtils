﻿using System;
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

        protected abstract List<TElement>? Parse( StatementLine srcLine );

        public string MatchText { get; }
        public ReadOnlyCollection<LineType> SupportedLineTypes => _lineTypes.AsReadOnly();

        public virtual bool HandlesLine( StatementLine srcLine ) => SupportedLineTypes.Any( x => x == srcLine.LineType )
                                                                 && _matcher.IsMatch( srcLine.Line );

        protected BaseInfo? GetParent(StatementLine srcLine, params ElementNature[] nature)
        {
            if( srcLine.Parent == null )
                return null;

            var curBlock = srcLine.Parent;
            BaseInfo? retVal = null;

            while (curBlock != null)
            {
                if( nature.All( x => curBlock.Elements?.FirstOrDefault()?.Nature != x ) )
                {
                    curBlock = curBlock.Parent;
                    continue;
                }

                retVal = curBlock.Elements!.First();
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
                            retVal.Add(sb.ToString().Trim());
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
                retVal.Add(sb.ToString().Trim());

            return retVal;
        }

        protected MethodSource ParseReturnTypeName(string text)
        {
            var numAngleBrackets = 0;
            var sb = new StringBuilder();
            string? methodName = null;
            string? returnType = null;
            string? typeArgs = null;

            foreach (var curChar in text)
            {
                switch (curChar)
                {
                    case ',':
                        sb.Append(curChar);
                        break;

                    case '<':
                        // if we're inside a type argument clause an opening
                        // angle bracket just means we're finding an embedded type argument
                        if( numAngleBrackets > 0 )
                            sb.Append( curChar );
                        else
                        {
                            //...but outside a type argument clause it means we have found 
                            // the start of a generic return type or the end of a method name
                            if( string.IsNullOrEmpty( returnType ) )
                                sb.Append( curChar );
                            else
                            {
                                methodName = sb.ToString();
                                sb.Clear();
                            }
                        }

                        numAngleBrackets++;

                        break;

                    case '>':
                        if( numAngleBrackets > 0)
                            sb.Append(curChar);

                        numAngleBrackets--;

                        // when the number of angle brackets goes to zero we're at the end
                        // of a type argument clause
                        if( numAngleBrackets == 0 )
                        {
                            if( string.IsNullOrEmpty( returnType ) )
                                returnType = sb.ToString();
                            else typeArgs = sb.ToString();

                            sb.Clear();
                        }

                        break;

                    case ' ':
                        // if we're not inside a type argument clause a space
                        // potentially means we've found the end of the return type (if it wasn't generic)
                        // or the method name.
                        if( numAngleBrackets <= 0 )
                        {
                            if( string.IsNullOrEmpty( returnType ) )
                                returnType = sb.ToString();
                            else methodName = sb.ToString();

                            sb.Clear();
                        }
                        else sb.Append( curChar );

                        break;

                    default:
                        sb.Append(curChar);
                        break;
                }
            }

            if( string.IsNullOrEmpty( methodName ) )
                methodName = sb.ToString();

            // for some reason my regex matcher returns an extra '>' at the
            // end of the type argument string
            if( !string.IsNullOrEmpty( typeArgs ) )
                typeArgs = typeArgs[ ..^1 ];

            return new MethodSource( methodName!, 
                string.Empty, 
                ParseArguments( typeArgs ), 
                new List<string>(),
                returnType! );
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

        List<BaseInfo>? IParse.Parse( StatementLine srcLine ) => !HandlesLine( srcLine ) ? null : Parse( srcLine )?.Cast<BaseInfo>().ToList();
    }
}