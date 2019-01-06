using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Expressions
{
    class PropertyMappingNameConverter : MappingNameConverter<PropertyMapping>
    {
        protected override bool ContainsMapping(ExpressionBuilder builder, PropertyMapping mapping)
        {
            var mappingBuilder = builder as PropertyMappingBuilder;
            return mappingBuilder != null && mappingBuilder.PropertyMappings.Contains(mapping);
        }
    }
}
