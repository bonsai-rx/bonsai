using OpenTK;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Shaders
{
    [Description("Returns the rotation component of the input matrix.")]
    public class ExtractRotation : Transform<Matrix4, Quaternion>
    {
        public ExtractRotation()
        {
            RowNormalize = true;
        }

        [Description("Indicates whether to row-normalize the input matrix. Keep this unless you know the input is already normalized.")]
        public bool RowNormalize { get; set; }

        public override IObservable<Quaternion> Process(IObservable<Matrix4> source)
        {
            return source.Select(input => input.ExtractRotation(RowNormalize));
        }
    }
}
