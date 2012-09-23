using Bonsai;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// TODO: replace this with the filter input type.
using TSource = System.String;

namespace Bonsai.ItemTemplates
{
    public class FilterTemplate : Filter<TSource>
    {
        public override bool Process(TSource input)
        {
            // TODO: process the input object and return a value indicating whether it meets the filter condition.
            throw new NotImplementedException();
        }
    }
}
