using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bonsai.Configuration
{
    [Serializable]
    public class PackageReferenceCollection : SortedKeyedCollection<string, PackageReference>
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
