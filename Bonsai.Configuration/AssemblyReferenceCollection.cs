using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bonsai.Configuration
{
    [Serializable]
    public class AssemblyReferenceCollection : SortedKeyedCollection<string, AssemblyReference>
    {
        public void Add(string name)
        {
            Add(new AssemblyReference(name));
        }

        protected override string GetKeyForItem(AssemblyReference item)
        {
            return item.AssemblyName;
        }
    }
}
