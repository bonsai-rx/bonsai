using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bonsai.Vision.Design
{
    public class IplImageOutputQuadrangleEditor : IplImageQuadrangleEditor
    {
        public IplImageOutputQuadrangleEditor()
            : base(DataSource.Output)
        {
        }
    }
}
