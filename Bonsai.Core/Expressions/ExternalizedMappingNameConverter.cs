using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Expressions
{
    class ExternalizedMappingNameConverter : MappingNameConverter<ExternalizedMapping>
    {
        protected override bool ContainsMapping(ExpressionBuilder builder, ExternalizedMapping mapping)
        {
            var mappingBuilder = builder as ExternalizedMappingBuilder;
            return mappingBuilder != null && mappingBuilder.ExternalizedProperties.Contains(mapping);
        }
    }
}
