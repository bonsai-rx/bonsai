using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Bonsai.Configuration
{
    public class PackageReferenceCollection : KeyedCollection<string, PackageReference>
    {
        public void Add(string id, string version)
        {
            Add(new PackageReference(id, version));
        }

        protected override string GetKeyForItem(PackageReference item)
        {
            return item.Id;
        }
    }
}
