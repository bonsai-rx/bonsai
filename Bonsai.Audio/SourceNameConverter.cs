using Bonsai.Resources;

namespace Bonsai.Audio
{
    class SourceNameConverter : ResourceNameConverter
    {
        public SourceNameConverter()
            : base(typeof(AudioSource))
        {
        }
    }
}
