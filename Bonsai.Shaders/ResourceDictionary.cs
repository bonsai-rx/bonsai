using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Resources
{
    [Obsolete]
    class ResourceDictionary<TResource> : IDictionary<string, TResource> where TResource : IDisposable
    {
        readonly ResourceManager manager;

        internal ResourceDictionary(ResourceManager resourceManager)
        {
            if (resourceManager == null)
            {
                throw new ArgumentNullException("resourceManager");
            }

            this.manager = resourceManager;
        }

        public void Add(string key, TResource value)
        {
            throw new NotSupportedException();
        }

        public bool ContainsKey(string key)
        {
            throw new NotImplementedException();
        }

        public ICollection<string> Keys
        {
            get { throw new NotImplementedException(); }
        }

        public bool Remove(string key)
        {
            throw new NotSupportedException();
        }

        public bool TryGetValue(string key, out TResource value)
        {
            try
            {
                value = manager.Load<TResource>(key);
                return true;
            }
            catch
            {
                value = default(TResource);
                return false;
            }
        }

        public ICollection<TResource> Values
        {
            get { throw new NotImplementedException(); }
        }

        public TResource this[string key]
        {
            get { return manager.Load<TResource>(key); }
            set { throw new NotSupportedException(); }
        }

        public void Add(KeyValuePair<string, TResource> item)
        {
            throw new NotSupportedException();
        }

        public void Clear()
        {
            throw new NotSupportedException();
        }

        public bool Contains(KeyValuePair<string, TResource> item)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(KeyValuePair<string, TResource>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public int Count
        {
            get { throw new NotImplementedException(); }
        }

        public bool IsReadOnly
        {
            get { return true; }
        }

        public bool Remove(KeyValuePair<string, TResource> item)
        {
            throw new NotSupportedException();
        }

        public IEnumerator<KeyValuePair<string, TResource>> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
