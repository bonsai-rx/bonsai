using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Resources
{
    struct ResourceKey : IEquatable<ResourceKey>
    {
        public Type Type;
        public string Name;

        public bool Equals(ResourceKey other)
        {
            return Type == other.Type && Name == other.Name;
        }

        public override bool Equals(object obj)
        {
            if (obj is ResourceKey)
            {
                return Equals((ResourceKey)obj);
            }

            return false;
        }

        public override int GetHashCode()
        {
            var hash = 103787;
            hash = hash * 82613 + EqualityComparer<Type>.Default.GetHashCode(Type);
            hash = hash * 82613 + EqualityComparer<string>.Default.GetHashCode(Name);
            return hash;
        }

        public override string ToString()
        {
            var name = Name;
            var typeName = Type.Name;
            if (string.IsNullOrEmpty(name)) return typeName;
            else return string.Format("{0} [{1}]", name, typeName);
        }
    }
}
