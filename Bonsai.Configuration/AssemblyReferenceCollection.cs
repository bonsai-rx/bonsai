using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Bonsai.Configuration
{
    public class AssemblyReferenceCollection : KeyedCollection<string, AssemblyReference>
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
