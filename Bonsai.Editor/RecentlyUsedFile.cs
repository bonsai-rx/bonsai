using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Editor
{
    class RecentlyUsedFile
    {
        public RecentlyUsedFile(DateTimeOffset timestamp, string fileName)
        {
            Timestamp = timestamp;
            FileName = fileName;
        }

        public DateTimeOffset Timestamp { get; private set; }

        public string FileName { get; private set; }
    }

    class RecentlyUsedFileCollection : IEnumerable<RecentlyUsedFile>
    {
        readonly SortedList<DateTimeOffset, RecentlyUsedFile> files;
        class DateTimeOffsetComparer : IComparer<DateTimeOffset>
        {
            public static readonly DateTimeOffsetComparer Default = new DateTimeOffsetComparer();

            public int Compare(DateTimeOffset x, DateTimeOffset y)
            {
                return y.CompareTo(x);
            }
        }

        public RecentlyUsedFileCollection(int capacity)
        {
            Capacity = capacity;
            files = new SortedList<DateTimeOffset, RecentlyUsedFile>(Capacity, DateTimeOffsetComparer.Default);
        }

        public int Capacity { get; private set; }

        public int Count
        {
            get { return files.Count; }
        }

        public void Add(string fileName)
        {
            var timestamp = DateTimeOffset.Now;
            Add(timestamp, fileName);
        }

        public void Add(DateTimeOffset timestamp, string fileName)
        {
            Remove(fileName);
            var item = new RecentlyUsedFile(timestamp, fileName);
            while (files.Count >= Capacity)
            {
                files.RemoveAt(files.Count - 1);
            }

            files.Add(timestamp, item);
        }

        public bool Remove(string fileName)
        {
            var item = files.Values.FirstOrDefault(value => value.FileName == fileName);
            if (item != null)
            {
                return files.Remove(item.Timestamp);
            }

            return false;
        }

        public IEnumerator<RecentlyUsedFile> GetEnumerator()
        {
            return files.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
