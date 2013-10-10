using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bonsai.Vision.Design
{
    public class IplImageInputRoiEditor : IplImageRoiEditor
    {
        public IplImageInputRoiEditor()
            : base(DataSource.Input)
        {
        }
    }
}
