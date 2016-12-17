using Bonsai;
using Bonsai.Design;
using Bonsai.Vision;
using Bonsai.Vision.Design;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[assembly: TypeVisualizer(typeof(CircleVisualizer), Target = typeof(Circle))]
[assembly: TypeVisualizer(typeof(CircleVisualizer), Target = typeof(Circle[]))]

namespace Bonsai.Vision.Design
{
    public class CircleVisualizer : ObjectTextVisualizer
    {
    }
}
