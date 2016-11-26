using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Shaders
{
    [Description("Updates the render state of the specified material.")]
    public class UpdateMaterialState : Sink
    {
        [Description("The name of the material.")]
        [Editor("Bonsai.Shaders.Configuration.Design.MaterialConfigurationEditor, Bonsai.Shaders.Design", typeof(UITypeEditor))]
        public string MaterialName { get; set; }

        [Description("Specifies whether the material is active.")]
        public bool Enabled { get; set; }

        public override IObservable<TSource> Process<TSource>(IObservable<TSource> source)
        {
            return source.CombineEither(
                ShaderManager.ReserveMaterial(MaterialName),
                (input, material) =>
                {
                    material.Update(() =>
                    {
                        material.Enabled = Enabled;
                    });
                    return input;
                });
        }
    }
}
