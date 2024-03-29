﻿using System;

namespace Bonsai.Configuration
{
    [Serializable]
    public class LibraryFolderCollection : SortedKeyedCollection<string, LibraryFolder>
    {
        public void Add(string path, string platform)
        {
            Add(new LibraryFolder(path, platform));
        }

        protected override string GetKeyForItem(LibraryFolder item)
        {
            return item.Path;
        }
    }
}
