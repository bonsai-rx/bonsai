using Bonsai;
using Bonsai.Design;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//TODO: Specify the target of the type visualizer
[assembly: TypeVisualizer(typeof($rootnamespace$.$safeitemname$), Target = typeof(object))]

namespace $rootnamespace$
{
    /// <summary>
    /// Provides a type visualizer that displays the values in a sequence.
    /// </summary>
    public class $safeitemname$ : DialogTypeVisualizer
    {
        /// <inheritdoc/>
        public override void Show(object value)
        {
            //TODO: Update the visualizer with a new value
        }

        /// <inheritdoc/>
        public override void Load(IServiceProvider provider)
        {
            var visualizerService = (IDialogTypeVisualizerService)provider.GetService(typeof(IDialogTypeVisualizerService));
            if (visualizerService != null)
            {
                //TODO: Load and add any user controls to the visualizer service
                //visualizerService.AddControl(mycontrol);
            }
        }

        /// <inheritdoc/>
        public override void Unload()
        {
            //TODO: Dispose of any user controls used by the visualizer
        }
    }
}
