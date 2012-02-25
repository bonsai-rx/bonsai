using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bonsai
{
    [AttributeUsage(AttributeTargets.Property)]
    public class FileNameFilterAttribute : Attribute
    {
        public FileNameFilterAttribute(string filter)
        {
            Filter = filter;
        }

        public string Filter { get; private set; }
    }
}
