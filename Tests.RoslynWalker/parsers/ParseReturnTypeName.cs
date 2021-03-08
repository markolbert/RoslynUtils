using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests.RoslynWalker
{
    public class ParseReturnTypeName : IParseReturnTypeName
    {
        private enum ParseState
        {
            Undefined,
            Attribute,
            ReturnType,
            Name,
            TypeArguments
        }

        public bool Parse( string text, out List<string> attributeClauses, out string? returnType, out string? name, out List<string> typeArgs )
        {
            var parseState = ParseState.Undefined;
            typeArgs = new List<string>();
            attributeClauses = new List<string>();
            returnType = null;
            name = null;

            var sb = new StringBuilder();
            var numAngleBrackets = 0;
            var numBrackets = 0;

            foreach( var curChar in text )
            {
                switch( curChar )
                {
                    case ',':
                        switch( parseState )
                        {
                            case ParseState.Undefined:
                            case ParseState.Name:
                                break;

                            case ParseState.ReturnType:
                                sb.Append( curChar );
                                break;

                            case ParseState.Attribute:
                            case ParseState.TypeArguments:
                                sb.Append( curChar );
                                break;
                        }

                        break;

                    case '<':
                        switch( parseState )
                        {
                            case ParseState.Undefined:
                            case ParseState.TypeArguments:
                                break;

                            case ParseState.Attribute:
                                sb.Append( curChar );
                                break;

                            case ParseState.Name:
                                // we've reached the end of the name clause
                                name = sb.ToString();
                                sb.Clear();

                                parseState = ParseState.TypeArguments;

                                break;
                        }

                        numAngleBrackets++;

                        break;

                    case '>':
                        numAngleBrackets--;

                        switch( numAngleBrackets )
                        {
                            case > 0:
                                sb.Append( curChar );
                                break;

                            // when the number of angle brackets goes to zero we're at the end
                            // of a type argument clause
                            case 0:
                                switch( parseState )
                                {
                                    case ParseState.Undefined:
                                    case ParseState.Name:
                                        // shouldn't happen; ignore
                                        break;

                                    case ParseState.Attribute:
                                        sb.Append( curChar );
                                        break;

                                    case ParseState.ReturnType:
                                        returnType = sb.ToString();
                                        sb.Clear();

                                        parseState = ParseState.Name;

                                        break;

                                    case ParseState.TypeArguments:
                                        typeArgs.Add(sb.ToString()  );
                                        sb.Clear();

                                        parseState = ParseState.Undefined;

                                        break;
                                }

                                break;
                        }

                        break;

                    case ' ':
                        switch( parseState )
                        {
                            case ParseState.Undefined:
                                break;

                            case ParseState.Attribute:
                            case ParseState.TypeArguments:
                                sb.Append( curChar );
                                break;

                            case ParseState.ReturnType:
                                if( numAngleBrackets == 0 )
                                {
                                    returnType = sb.ToString();
                                    sb.Clear();

                                    parseState = ParseState.Name;
                                }
                                else sb.Append( curChar );

                                break;

                            case ParseState.Name:
                                name = sb.ToString();
                                sb.Clear();

                                parseState = ParseState.TypeArguments;
                                break;
                        }

                        break;

                    case '[':
                        switch( parseState )
                        {
                            case ParseState.Undefined:
                                parseState = ParseState.Attribute;
                                break;

                            case ParseState.Name:
                            case ParseState.Attribute:
                                break;

                            case ParseState.ReturnType:
                            case ParseState.TypeArguments:
                                sb.Append( curChar );
                                break;
                        }

                        numBrackets++;

                        break;

                    case ']':
                        numBrackets--;

                        switch( parseState )
                        {
                            case ParseState.Undefined:
                            case ParseState.Name:
                                break;

                            case ParseState.Attribute:
                                if( numBrackets <= 0 )
                                {
                                    attributeClauses.Add( sb.ToString() );
                                    sb.Clear();

                                    parseState = ParseState.ReturnType;
                                }
                                else sb.Append( curChar );

                                break;

                            case ParseState.ReturnType:
                                sb.Append( curChar );

                                if( numBrackets == 0 )
                                {
                                    returnType = sb.ToString();
                                    sb.Clear();

                                    parseState = ParseState.Name;
                                }

                                break;

                            case ParseState.TypeArguments:
                                sb.Append( curChar );
                                break;
                        }

                        break;

                    default:
                        if( parseState == ParseState.Undefined )
                            parseState = ParseState.ReturnType;

                        sb.Append( curChar );
                        break;
                }
            }

            // anything left in sb at this point are the type arguments
            typeArgs.AddRange( ParseTypeArguments( sb.ToString() ) );

            return !string.IsNullOrEmpty( name ) && !string.IsNullOrEmpty( returnType );
        }

        private List<string> ParseTypeArguments( string text )
        {
            var retVal = new List<string>();

            if( text.Length == 0 )
                return retVal;

            var numAngleBrackets = 0;
            var sb = new StringBuilder();

            foreach( var curChar in text )
            {
                switch( curChar )
                {
                    case ',':
                        if( numAngleBrackets > 0 )
                            sb.Append( curChar );
                        else
                        {
                            retVal.Add( sb.ToString() );
                            sb.Clear();
                        }

                        break;

                    case '<':
                        numAngleBrackets++;
                        sb.Append( curChar );

                        break;

                    case '>':
                        sb.Append( curChar );
                        numAngleBrackets--;

                        break;

                    case ' ':
                        if( numAngleBrackets > 0 )
                            sb.Append( curChar );

                        break;

                    default:
                        sb.Append( curChar );
                        break;
                }
            }

            // any remaining text in sb is the last type argument
            if( sb.Length > 0 )
                retVal.Add( sb.ToString() );

            return retVal;
        }
    }
}
