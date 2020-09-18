using OpenCV.Net;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Shaders
{
    class TextureSequence : TextureArray, ITextureSequence
    {
        int index = 0;
        readonly bool looping;

        public TextureSequence(int bufferLength, bool loop)
            : base(bufferLength)
        {
            looping = loop;
        }

        public double PlaybackRate { get; set; }

        public bool MoveNext()
        {
            if (index >= Length)
            {
                if (looping) index = 0;
                else return false;
            }

            SetActiveTexture(index);
            index = index + 1;
            return true;
        }

        public void Reset()
        {
            index = 0;
            Id = 0;
        }
    }
}
