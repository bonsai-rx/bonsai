using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Bonsai.Configuration
{
    public class AssemblyLocationCollection : KeyedCollection<string, AssemblyLocation>
    {
        public void Add(string name, string path)
        {
            Add(new AssemblyLocation(name, path));
        }

        protected override string GetKeyForItem(AssemblyLocation item)
        {
            return item.Name;
        }
    }
}
