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
