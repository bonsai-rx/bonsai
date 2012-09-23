using Bonsai;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// TODO: replace these with the projection input and output types.
using TSource = System.String;
using TResult = System.String;

namespace Bonsai.ItemTemplates
{
    public class ProjectionTemplate : Projection<TSource, TResult>
    {
        public override TResult Process(TSource input)
        {
            // TODO: process the input object and return the result data.
            throw new NotImplementedException();
        }
    }
}
