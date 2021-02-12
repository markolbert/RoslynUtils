#region license

// Copyright 2021 Mark A. Olbert
// 
// This library or program 'RoslynDb' is free software: you can redistribute it
// and/or modify it under the terms of the GNU General Public License as
// published by the Free Software Foundation, either version 3 of the License,
// or (at your option) any later version.
// 
// This library or program is distributed in the hope that it will be useful, but
// WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// General Public License for more details.
// 
// You should have received a copy of the GNU General Public License along with
// this library or program.  If not, see <https://www.gnu.org/licenses/>.

#endregion

using System.Text;
using J4JSoftware.Logging;
using J4JSoftware.Utilities;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn
{
    public class AttributeProcessor : SimpleProcessorDb<ISymbol>
    {
        public AttributeProcessor(
            IRoslynDataLayer dataLayer,
            ActionsContext context,
            IJ4JLogger? logger )
            : base( "adding Attributes to the database", dataLayer, context, logger )
        {
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
            {
                sb.Append( typedConst.Value );
            }
            else
            {
                if( typedConst.Values != null )
                    foreach( var tcValue in typedConst.Values )
                    {
                        if( sb.Length > 0 )
                            sb.Append( ", " );

                        sb.Append( tcValue.ToString() );
                    }
            }

            if( sb.Length == 0 )
                Logger?.Error<string>( "Couldn't retrieve value for Attribute property or argument '{0}'",
                    typedConst.Type?.ToUniqueName() ?? "<null type>" );

            return sb.ToString();
        }
    }
}