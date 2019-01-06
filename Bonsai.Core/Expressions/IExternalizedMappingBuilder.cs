using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Expressions
{
    internal interface IExternalizedMappingBuilder
    {
        IEnumerable<ExternalizedMapping> GetExternalizedProperties();
    }
}
