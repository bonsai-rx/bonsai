using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bonsai.Vision.Design
{
    public class IplImageInputQuadrangleEditor : IplImageQuadrangleEditor
    {
        public IplImageInputQuadrangleEditor()
            : base(DataSource.Input)
        {
        }
    }
}
