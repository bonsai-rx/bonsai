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
