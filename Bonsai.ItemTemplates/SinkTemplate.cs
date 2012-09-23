using Bonsai;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// TODO: replace this with the sink input type.
using TSource = System.String;

namespace Bonsai.ItemTemplates
{
    public class SinkTemplate : Sink<TSource>
    {
        public override void Process(TSource input)
        {
            // TODO: process the input object.
            throw new NotImplementedException();
        }
    }
}
