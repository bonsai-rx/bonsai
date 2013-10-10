using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bonsai.Vision.Design
{
    public class IplImageInputRectangleEditor : IplImageRectangleEditor
    {
        public IplImageInputRectangleEditor()
            : base(DataSource.Input)
        {
        }
    }
}
