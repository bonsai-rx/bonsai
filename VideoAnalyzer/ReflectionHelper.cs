using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace VideoAnalyzer
{
    public static class ReflectionHelper
    {
        public static IEnumerable<Type> GetAssemblyTypes()
        {
            return GetAssemblyTypes(Assembly.GetCallingAssembly());
        }

        public static IEnumerable<Type> GetAssemblyTypes(Assembly assembly)
        {
            var types = assembly.GetTypes();
            for (int i = 0; i < types.Length; i++)
            {
                var type = types[i];
                if (type.IsAbstract) continue;

                yield return type;
            }
        }

        public static bool MatchGenericType(Type type, Type genericType)
        {
            if (!genericType.IsGenericType)
            {
                throw new ArgumentException("Trying to match against a non-generic type.", "genericType");
            }

            while (type != null)
            {
                if (type.IsGenericType && type.GetGenericTypeDefinition() == genericType)
                {
                    return true;
                }

                type = type.BaseType;
            }

            return false;
        }
    }
}
