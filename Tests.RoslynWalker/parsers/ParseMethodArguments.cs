using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests.RoslynWalker
{
    public class ParseMethodArguments : IParseMethodArguments
    {
        private enum ParseState
        {
            Undefined,
            Attribute,
            ArgumentType,
            ArgumentName
        }

        public bool Parse( string text, out List<ArgumentSource> arguments )
        {
            var parseState = ParseState.Undefined;
            var attributeClauses = new List<string>();
            arguments = new List<ArgumentSource>();

            var sb = new StringBuilder();
            string? argType = null;
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
                                break;

                            case ParseState.ArgumentType:
                                sb.Append( curChar );
                                break;

                            case ParseState.ArgumentName:
                                // we've reached the end of an argument type/name
                                arguments.Add( new ArgumentSource( sb.ToString(), argType!, attributeClauses ) );
                                sb.Clear();
                                attributeClauses.Clear();
                                argType = null;

                                parseState = ParseState.Undefined;

                                break;

                            case ParseState.Attribute:
                                sb.Append( curChar );
                                break;
                        }

                        break;

                    case '<':
                        switch( parseState )
                        {
                            case ParseState.Undefined:
                            case ParseState.ArgumentName:
                                // shouldn't happen; ignore
                                break;

                            case ParseState.ArgumentType:
                            case ParseState.Attribute:
                                sb.Append( curChar );
                                break;
                        }

                        numAngleBrackets++;

                        break;

                    case '>':
                        if( numAngleBrackets > 0 )
                            sb.Append( curChar );

                        numAngleBrackets--;

                        // when the number of angle brackets goes to zero we're at the end
                        // of a type argument clause
                        if( numAngleBrackets == 0 )
                        {
                            switch( parseState )
                            {
                                case ParseState.Undefined:
                                case ParseState.ArgumentName:
                                    // shouldn't happen; ignore
                                    break;

                                case ParseState.Attribute:
                                    sb.Append( curChar );
                                    break;

                                case ParseState.ArgumentType:
                                    argType = sb.ToString();
                                    sb.Clear();

                                    parseState = ParseState.ArgumentName;

                                    break;
                            }
                        }

                        break;

                    case ' ':
                        switch( parseState )
                        {
                            case ParseState.Undefined:
                                break;

                            case ParseState.Attribute:
                                sb.Append( curChar );
                                break;

                            case ParseState.ArgumentType:
                                if( numAngleBrackets == 0 )
                                {
                                    argType = sb.ToString();
                                    sb.Clear();

                                    parseState = ParseState.ArgumentName;
                                }
                                else sb.Append( curChar );

                                break;

                            case ParseState.ArgumentName:
                                arguments.Add(new ArgumentSource(sb.ToString(), argType!, attributeClauses)  );
                                sb.Clear();
                                attributeClauses.Clear();
                                argType = null;

                                parseState = ParseState.Undefined;
                                break;
                        }

                        break;

                    case '[':
                        switch( parseState )
                        {
                            case ParseState.Undefined:
                                parseState = ParseState.Attribute;
                                break;

                            case ParseState.ArgumentName:
                            case ParseState.Attribute:
                                break;

                            case ParseState.ArgumentType:
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
                            case ParseState.ArgumentName:
                                break;

                            case ParseState.Attribute:
                                if( numBrackets <= 0 )
                                {
                                    attributeClauses.Add( sb.ToString() );
                                    sb.Clear();

                                    parseState = ParseState.Undefined;
                                }
                                else sb.Append( curChar );

                                break;

                            case ParseState.ArgumentType:
                                sb.Append( curChar );
                                break;
                        }

                        break;

                    default:
                        if( parseState == ParseState.Undefined )
                            parseState = ParseState.ArgumentType;

                        sb.Append( curChar );
                        break;
                }
            }

            return true;
        }
    }
}
