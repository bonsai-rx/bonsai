using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Bonsai.Configuration
{
    public class LibraryFolderCollection : KeyedCollection<string, LibraryFolder>
    {
        public void Add(string path)
        {
            Add(new LibraryFolder(path));
        }

        protected override string GetKeyForItem(LibraryFolder item)
        {
            return item.Path;
        }
    }
}
