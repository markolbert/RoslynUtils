using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.XPath;
using J4JSoftware.Logging;
using J4JSoftware.Utilities;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn
{
    public class AttributeProcessor : BaseProcessorDb<ISymbol, ISymbol>
    {
        public AttributeProcessor(
            IRoslynDataLayer dataLayer,
            ActionsContext context,
            IJ4JLogger logger)
            : base("adding Attributes to the database", dataLayer, context, logger)
        {
        }

        protected override List<ISymbol> ExtractSymbols( IEnumerable<ISymbol> inputData )
        {
            return inputData.ToList();
        }

        protected override bool ProcessSymbol( ISymbol symbol )
        {
            var targetDb = DataLayer.GetAttributableEntity( symbol );

            if( targetDb == null )
                return false;

            foreach( var attrData in symbol.GetAttributes() )
            {
                if( attrData == null )
                    continue;

                var attrDb = DataLayer.GetAttribute( targetDb, attrData, true );
                if( attrDb == null )
                    return false;

                for( var idx = 0; idx < attrData.ConstructorArguments.Length; idx++ )
                {
                    var attrArgDb = DataLayer.GetAttributeArgument( attrDb, attrData, idx, true );
                    if( attrArgDb == null )
                        return false;

                    attrArgDb.Value = AttributeArgumentValueToString( attrData.ConstructorArguments[ idx ] );
                }

                foreach( var kvp in attrData.NamedArguments )
                {
                    var attrArgDb = DataLayer.GetAttributeArgument( attrDb, attrData, kvp.Key, true );
                    if( attrArgDb == null )
                        return false;

                    attrArgDb.Value = AttributeArgumentValueToString( kvp.Value );
                }
            }

            return true;
        }

        private string AttributeArgumentValueToString( TypedConstant typedConst )
        {
            var sb = new StringBuilder();

            if( typedConst.Value != null )
                sb.Append( typedConst.Value.ToString() );
            else
            {
                if( typedConst.Values != null )
                {
                    foreach( var tcValue in typedConst.Values )
                    {
                        if( sb.Length > 0 )
                            sb.Append( ", " );

                        sb.Append( tcValue.ToString() );
                    }
                }
            }

            if( sb.Length == 0 )
                Logger.Error<string>( "Couldn't retrieve value for Attribute property or argument '{0}'",
                    typedConst.Type.ToUniqueName() );

            return sb.ToString();
        }
    }
}
