using System.Collections.Generic;

namespace Bonsai.Expressions
{
    internal interface IExternalizedMappingBuilder
    {
        IEnumerable<ExternalizedMapping> GetExternalizedProperties();
    }
}
