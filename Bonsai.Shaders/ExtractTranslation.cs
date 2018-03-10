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
    [Description("Returns the translation component of the input matrix.")]
    public class ExtractTranslation : Transform<Matrix4, Vector3>
    {
        public override IObservable<Vector3> Process(IObservable<Matrix4> source)
        {
            return source.Select(input => input.ExtractTranslation());
        }
    }
}
