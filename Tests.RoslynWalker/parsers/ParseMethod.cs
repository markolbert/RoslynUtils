using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Tests.RoslynWalker
{
    public class ParseMethod : ParseBase<MethodInfo>
    {
        private static readonly Regex _rxMethodArgs =
            new( @"(.*)(?<paren>[(])(.*)(?<-paren>[)])", RegexOptions.Compiled );

        private static readonly Regex _rxMethodArgsGroup = new(@$"\s*([^()]*)\(\s*(.*)\)");
        private static readonly Regex _rxMethodGroup = new(
            @$"\s*({AccessibilityClause})?(.*)\((.*)\)$",
            RegexOptions.Compiled);

        public ParseMethod()
            : base( ElementNature.Method, @".*\(.*\)", ParserFocus.CurrentSourceLine, LineType.Statement )
        {
        }

        protected override List<MethodInfo>? Parse( SourceLine srcLine )
        {
            if (!ExtractMethodElements(srcLine.Line, out var methodSrc))
                return null;

            var info= new MethodInfo( methodSrc! )
            {
                Parent = GetParent( srcLine, ElementNature.Class, ElementNature.Interface )
            };

            return new List<MethodInfo> { info };
        }

        protected bool ExtractMethodElements( string text, out MethodSource? result)
        {
            result = null;

            var groupMatch = _rxMethodGroup.Match(text);

            if (!groupMatch.Success
                || groupMatch.Groups.Count != 4)
                return false;

            var returnNameGenericSource = ParseReturnTypeName( groupMatch.Groups[ 2 ].Value.Trim() );

            result = returnNameGenericSource with
            {
                Accessibility = groupMatch.Groups[ 1 ].Value.Trim(),
                Arguments = ParseArguments( groupMatch.Groups[ 3 ].Value.Trim() )
            };

            return true;
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

    }
}