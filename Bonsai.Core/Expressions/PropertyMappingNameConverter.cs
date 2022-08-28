namespace Bonsai.Expressions
{
    class PropertyMappingNameConverter : MappingNameConverter<PropertyMapping>
    {
        protected override bool ContainsMapping(ExpressionBuilder builder, PropertyMapping mapping)
        {
            return builder is PropertyMappingBuilder mappingBuilder && mappingBuilder.PropertyMappings.Contains(mapping);
        }
    }
}
