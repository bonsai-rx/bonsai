using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bonsai.Vision.Design
{
    public class IplImageOutputRectangleEditor : IplImageRectangleEditor
    {
        public IplImageOutputRectangleEditor()
            : base(DataSource.Output)
        {
        }
    }
}
