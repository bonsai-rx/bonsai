using Bonsai.Resources;

namespace Bonsai.Audio
{
    class BufferNameConverter : ResourceNameConverter
    {
        public BufferNameConverter()
            : base(typeof(Buffer))
        {
        }
    }
}
