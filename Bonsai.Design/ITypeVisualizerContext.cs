using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Bonsai.Expressions;

namespace Bonsai.Design
{
    public interface ITypeVisualizerContext
    {
        InspectBuilder Source { get; }
    }
}
