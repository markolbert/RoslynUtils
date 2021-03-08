using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests.RoslynWalker
{
    public class ParseAttribute : IParseAttribute
    {
        private enum ParseState
        {
            Undefined,
            Name,
            Arguments
        }
        
        public bool Parse( List<string> clauses, out List<AttributeSource> result )
        {
            result = new List<AttributeSource>();

            if( clauses.Count == 0 )
                return true;

            foreach( var clause in clauses )
            {
                if( !Parse( clause, out var attributeSource ) )
                    return false;

                result.Add( attributeSource! );
            }

            return true;
        }

        private bool Parse( string text, out AttributeSource? result )
        {
            result = null;

            var sb = new StringBuilder();
            string? argName = null;
            var parseState = ParseState.Undefined;
            var numAngleBrackets = 0;
            var numBrackets = 0;
            List<AttributeArgumentSource>? curAttrArgs = null;

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

                            case ParseState.Arguments:
                                if( string.IsNullOrEmpty( argName ) )
                                {
                                    argName = sb.ToString();
                                    sb.Clear();
                                }

                                curAttrArgs!.Add( new AttributeArgumentSource( argName, sb.ToString() ) );
                                argName = null;
                                sb.Clear();

                                break;
                        }

                        break;

                    case '(':
                        switch( parseState )
                        {
                            case ParseState.Undefined:
                                break;

                            case ParseState.Arguments:
                                sb.Append( curChar );
                                break;

                            case ParseState.Name:
                                curAttrArgs = new();

                                result = new AttributeSource( sb.ToString(), curAttrArgs );
                                sb.Clear();

                                parseState = ParseState.Arguments;

                                break;
                        }

                        break;

                    case ')':
                        if( parseState != ParseState.Arguments )
                            sb.Append( curChar );

                        break;

                    case '[':
                        numBrackets++;
                        sb.Append( curChar );

                        break;

                    case ']':
                        numBrackets--;
                        sb.Append( curChar );

                        break;

                    case '<':
                        numAngleBrackets++;
                        sb.Append( curChar );

                        break;

                    case '>':
                        numAngleBrackets--;
                        sb.Append( curChar );

                        break;

                    case ' ':
                        switch( parseState )
                        {
                            case ParseState.Undefined:
                            case ParseState.Name:
                                break;

                            case ParseState.Arguments:
                                if( numAngleBrackets + numBrackets > 0 )
                                    sb.Append( curChar );

                                break;
                        }

                        break;

                    default:
                        if( parseState == ParseState.Undefined )
                            parseState = ParseState.Name;

                        sb.Append( curChar );
                        break;
                }
            }

            // anything left in sb at this point is the last argument
            if( string.IsNullOrEmpty( argName ) )
            {
                argName = sb.ToString();
                sb.Clear();
            }

            curAttrArgs!.Add( new AttributeArgumentSource( argName, sb.ToString() ) );

            return result != null;
        }
    }
}
