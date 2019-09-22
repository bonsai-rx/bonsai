using Bonsai.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
