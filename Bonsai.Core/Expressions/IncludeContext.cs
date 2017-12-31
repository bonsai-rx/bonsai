using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Expressions
{
    class IncludeContext : GroupContext
    {
        string includePath;

        public IncludeContext(IBuildContext parentContext, string path)
            : base(parentContext)
        {
            includePath = path;
        }

        public string Path
        {
            get { return includePath; }
        }
    }
}
