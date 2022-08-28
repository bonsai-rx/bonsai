using System;
using System.Collections.ObjectModel;
using System.Xml.Serialization;

namespace Bonsai.Audio
{
    /// <summary>
    /// Represents a collection of audio context configuration settings.
    /// </summary>
    [Obsolete]
    [XmlRoot("AudioContextSettings")]
    public class AudioContextConfigurationCollection : KeyedCollection<string, AudioContextConfiguration>
    {
        /// <inheritdoc/>
        protected override string GetKeyForItem(AudioContextConfiguration item)
        {
            return item.DeviceName;
        }
    }
}
