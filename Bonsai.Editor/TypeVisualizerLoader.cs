using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.IO;

namespace Bonsai.Editor
{
    public class TypeVisualizerLoader : MarshalByRefObject
    {
        [Serializable]
        class TypeVisualizerInfo
        {
            public string VisualizerTypeName;
            public string TargetTypeName;

            public TypeVisualizerInfo(TypeVisualizerAttribute typeVisualizer)
            {
                TargetTypeName = typeVisualizer.TargetTypeName;
                VisualizerTypeName = typeVisualizer.VisualizerTypeName;
            }
        }

        IEnumerable<TypeVisualizerAttribute> GetCustomAttributeTypes(Assembly assembly, Type attributeType)
        {
            Type[] types;
            var typeVisualizers = Enumerable.Empty<TypeVisualizerAttribute>();

            try { types = assembly.GetTypes(); }
            catch (ReflectionTypeLoadException) { return typeVisualizers; }

            for (int i = 0; i < types.Length; i++)
            {
                var visualizerAttributes = types[i].GetCustomAttributes(attributeType, true).Cast<TypeVisualizerAttribute>();
                typeVisualizers = typeVisualizers.Concat(visualizerAttributes);
            }

            return typeVisualizers;
        }

        TypeVisualizerInfo[] GetReflectionTypeVisualizerAttributes()
        {
            var typeVisualizers = Enumerable.Empty<TypeVisualizerAttribute>();
            var typeVisualizerAttributeAssembly = Assembly.Load(typeof(TypeVisualizerAttribute).Assembly.FullName);
            var typeVisualizerAttributeType = typeVisualizerAttributeAssembly.GetType(typeof(TypeVisualizerAttribute).FullName);

            var files = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, "*.dll");
            for (int i = 0; i < files.Length; i++)
            {
                try
                {
                    var assembly = Assembly.LoadFrom(files[i]);
                    var visualizerAttributes = assembly.GetCustomAttributes(typeVisualizerAttributeType, true).Cast<TypeVisualizerAttribute>();
                    typeVisualizers = typeVisualizers.Concat(visualizerAttributes);

                    typeVisualizers = typeVisualizers.Concat(GetCustomAttributeTypes(assembly, typeVisualizerAttributeType));
                }
                catch (FileLoadException) { continue; }
                catch (FileNotFoundException) { continue; }
                catch (BadImageFormatException) { continue; }
            }

            return typeVisualizers.Distinct().Select(data => new TypeVisualizerInfo(data)).ToArray();
        }

        public static Dictionary<Type, Type> GetTypeVisualizerDictionary()
        {
            var reflectionDomain = AppDomain.CreateDomain("ReflectionOnly");
            try
            {
                var loader = (TypeVisualizerLoader)reflectionDomain.CreateInstanceAndUnwrap(typeof(TypeVisualizerLoader).Assembly.FullName, typeof(TypeVisualizerLoader).FullName);
                var typeVisualizers = loader.GetReflectionTypeVisualizerAttributes();

                var visualizerMap = new Dictionary<Type, Type>(typeVisualizers.Length);
                for (int i = 0; i < typeVisualizers.Length; i++)
                {
                    var key = Type.GetType(typeVisualizers[i].TargetTypeName);
                    var value = Type.GetType(typeVisualizers[i].VisualizerTypeName);
                    visualizerMap.Add(key, value);
                }

                return visualizerMap;
            }
            finally { AppDomain.Unload(reflectionDomain); }
        }
    }
}
