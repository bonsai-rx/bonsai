using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Shaders
{
    public class Bounds
    {
        public static readonly Bounds Empty = new Bounds(Vector3.Zero, Vector3.Zero);
        Vector3 center;
        Vector3 extents;

        public Bounds(Vector3 center, Vector3 extents)
        {
            this.center = center;
            this.extents = extents;
        }

        public Vector3 Center
        {
            get { return center; }
        }

        public Vector3 Extents
        {
            get { return extents; }
        }

        public Vector3 Minimum
        {
            get { return center - extents; }
        }

        public Vector3 Maximum
        {
            get { return center + extents; }
        }

        public Vector3 Size
        {
            get { return 2 * extents; }
        }

        public override string ToString()
        {
            return string.Format("(Center:{0}, Extents:{1})", center, extents);
        }
    }
}
