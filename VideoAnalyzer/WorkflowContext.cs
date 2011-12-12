using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Design;

namespace VideoAnalyzer
{
    public class WorkflowContext : ServiceContainer
    {
        public WorkflowContext()
        {
        }

        public WorkflowContext(IServiceProvider parentProvider)
            : base(parentProvider)
        {
        }
    }
}
