using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.ComponentModel.Design;
using System.Xml.Serialization;

namespace Bonsai.Design
{
    public abstract class DialogMashupVisualizer : DialogTypeVisualizer
    {
        readonly Collection<VisualizerMashup> mashups = new Collection<VisualizerMashup>();

        [XmlIgnore]
        public Collection<VisualizerMashup> Mashups
        {
            get { return mashups; }
        }

        public override void Load(IServiceProvider provider)
        {
            LoadMashups(provider);
        }

        public override void Unload()
        {
            UnloadMashups();
        }

        public void LoadMashups(IServiceProvider provider)
        {
            using (var serviceContainer = new ServiceContainer(provider))
            {
                serviceContainer.AddService(typeof(DialogMashupVisualizer), this);
                foreach (var mashup in mashups)
                {
                    mashup.Visualizer.Load(serviceContainer);
                }
                serviceContainer.RemoveService(typeof(DialogMashupVisualizer));
            }
        }

        public void UnloadMashups()
        {
            foreach (var mashup in mashups)
            {
                mashup.Visualizer.Unload();
            }
        }
    }
}
