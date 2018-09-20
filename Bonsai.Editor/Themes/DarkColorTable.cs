using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Editor.Themes
{
    class DarkColorTable : MapColorTable
    {
        public DarkColorTable()
            : base(new LightColorTable(), ThemeHelper.Invert)
        {
        }
    }
}
