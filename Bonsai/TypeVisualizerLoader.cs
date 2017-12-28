using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.IO;
using System.Reactive.Linq;
using Bonsai.Configuration;
using Bonsai.Editor;

namespace Bonsai
{
    sealed class TypeVisualizerLoader : MarshalByRefObject
    {
        public TypeVisualizerLoader(PackageConfiguration configuration)
        {
            ConfigurationHelper.SetAssemblyResolve(configuration);
        }

        static IEnumerable<TypeVisualizerAttribute> GetCustomAttributeTypes(Assembly assembly)
        {
            Type[] types;
            var typeVisualizers = Enumerable.Empty<TypeVisualizerAttribute>();

            try { types = assembly.GetTypes(); }
            catch (ReflectionTypeLoadException) { return typeVisualizers; }

            for (int i = 0; i < types.Length; i++)
            {
                var type = types[i];
                if (type.IsPublic && !type.IsAbstract && !type.ContainsGenericParameters)
                {
                    var visualizerAttributes = Array.ConvertAll(type.GetCustomAttributes(typeof(TypeVisualizerAttribute), true), attribute =>
                    {
                        var visualizerAttribute = (TypeVisualizerAttribute)attribute;
                        visualizerAttribute.TargetTypeName = type.AssemblyQualifiedName;
                        return visualizerAttribute;
                    });

                    if (visualizerAttributes.Length > 0)
                    {
                        typeVisualizers = typeVisualizers.Concat(visualizerAttributes);
                    }
                }
            }

            return typeVisualizers;
        }

        TypeVisualizerDescriptor[] GetReflectionTypeVisualizerAttributes(string assemblyRef)
        {
            var typeVisualizers = Enumerable.Empty<TypeVisualizerAttribute>();
            try
            {
                var assembly = Assembly.Load(assemblyRef);
                var visualizerAttributes = assembly.GetCustomAttributes(typeof(TypeVisualizerAttribute), true).Cast<TypeVisualizerAttribute>();
                typeVisualizers = typeVisualizers.Concat(visualizerAttributes);
                typeVisualizers = typeVisualizers.Concat(GetCustomAttributeTypes(assembly));
            }
            catch (FileLoadException) { }
            catch (FileNotFoundException) { }
            catch (BadImageFormatException) { }

            return typeVisualizers.Distinct().Select(data => new TypeVisualizerDescriptor(data)).ToArray();
        }

        public static IObservable<TypeVisualizerDescriptor> GetTypeVisualizerDictionary(PackageConfiguration configuration)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException("configuration");
            }

            var assemblies = configuration.AssemblyReferences.Select(reference => reference.AssemblyName);
            return Observable.Using(
                () => new LoaderResource<TypeVisualizerLoader>(configuration),
                resource => from assemblyRef in assemblies.ToObservable()
                            let typeVisualizers = resource.Loader.GetReflectionTypeVisualizerAttributes(assemblyRef)
                            from typeVisualizer in typeVisualizers
                            select typeVisualizer);
        }
    }
}
