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

            public TypeVisualizerInfo(CustomAttributeData typeVisualizerAttributeData)
            {
                var constructorArgument = typeVisualizerAttributeData.ConstructorArguments[0];
                if (constructorArgument.ArgumentType == typeof(Type))
                {
                    var type = (Type)constructorArgument.Value;
                    VisualizerTypeName = type != null ? type.AssemblyQualifiedName : null;
                }
                else VisualizerTypeName = constructorArgument.Value != null ? constructorArgument.Value.ToString() : null;

                foreach (var namedArgument in typeVisualizerAttributeData.NamedArguments)
                {
                    var typedValue = namedArgument.TypedValue;
                    if (typedValue.ArgumentType == typeof(Type))
                    {
                        var type = (Type)typedValue.Value;
                        TargetTypeName = type != null ? type.AssemblyQualifiedName : null;
                    }
                    else TargetTypeName = typedValue.Value != null ? typedValue.Value.ToString() : null;
                }
            }
        }

        IEnumerable<CustomAttributeData> GetCustomAttributeTypes(Assembly assembly, Type attributeClass)
        {
            var types = assembly.GetTypes();
            var typeVisualizers = Enumerable.Empty<CustomAttributeData>();
            for (int i = 0; i < types.Length; i++)
            {
                var visualizerAttributes = types[i].GetCustomAttributesData().Where(data => data.Constructor.DeclaringType == attributeClass);
                typeVisualizers = typeVisualizers.Concat(visualizerAttributes);
            }

            return typeVisualizers;
        }

        TypeVisualizerInfo[] GetReflectionOnlyTypeVisualizerAttributes()
        {
            var typeVisualizers = Enumerable.Empty<CustomAttributeData>();
            var typeVisualizerAttributeAssembly = Assembly.ReflectionOnlyLoad(typeof(TypeVisualizerAttribute).Assembly.FullName);
            var typeVisualizerAttributeType = typeVisualizerAttributeAssembly.GetType(typeof(TypeVisualizerAttribute).FullName);

            var files = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, "*.dll");
            for (int i = 0; i < files.Length; i++)
            {
                try
                {
                    var assembly = Assembly.ReflectionOnlyLoadFrom(files[i]);
                    var visualizerAttributes = assembly.GetCustomAttributesData().Where(data => data.Constructor.DeclaringType == typeVisualizerAttributeType);
                    typeVisualizers = typeVisualizers.Concat(visualizerAttributes);

                    typeVisualizers = typeVisualizers.Concat(GetCustomAttributeTypes(assembly, typeVisualizerAttributeType));
                }
                catch (BadImageFormatException) { continue; }
            }

            return typeVisualizers.Distinct().Select(data => new TypeVisualizerInfo(data)).ToArray();
        }

        public static Dictionary<Type, Type> GetTypeVisualizerDictionary()
        {
            var reflectionOnlyDomain = AppDomain.CreateDomain("ReflectionOnly");
            try
            {
                reflectionOnlyDomain.ReflectionOnlyAssemblyResolve += new ResolveEventHandler(reflectionOnlyDomain_ReflectionOnlyAssemblyResolve);
                var loader = (TypeVisualizerLoader)reflectionOnlyDomain.CreateInstanceAndUnwrap(typeof(TypeVisualizerLoader).Assembly.FullName, typeof(TypeVisualizerLoader).FullName);
                var typeVisualizers = loader.GetReflectionOnlyTypeVisualizerAttributes();

                var visualizerMap = new Dictionary<Type, Type>(typeVisualizers.Length);
                for (int i = 0; i < typeVisualizers.Length; i++)
                {
                    var key = Type.GetType(typeVisualizers[i].TargetTypeName);
                    var value = Type.GetType(typeVisualizers[i].VisualizerTypeName);
                    visualizerMap.Add(key, value);
                }

                return visualizerMap;
            }
            finally { AppDomain.Unload(reflectionOnlyDomain); }
        }

        static Assembly reflectionOnlyDomain_ReflectionOnlyAssemblyResolve(object sender, ResolveEventArgs args)
        {
            return Assembly.ReflectionOnlyLoad(args.Name);
        }
    }
}
