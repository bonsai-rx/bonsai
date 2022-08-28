namespace Bonsai.Expressions
{
    class ExternalizedMappingNameConverter : MappingNameConverter<ExternalizedMapping>
    {
        protected override bool ContainsMapping(ExpressionBuilder builder, ExternalizedMapping mapping)
        {
            return builder is ExternalizedMappingBuilder mappingBuilder && mappingBuilder.ExternalizedProperties.Contains(mapping);
        }
    }
}
