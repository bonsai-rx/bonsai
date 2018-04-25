using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Design
{
    public abstract class WorkflowIcon
    {
        internal WorkflowIcon()
        {
        }

        public abstract string Name { get; }

        public abstract Stream GetStream();
    }
}
