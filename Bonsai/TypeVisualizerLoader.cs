using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.IO;
using System.Reactive.Linq;
using Bonsai.Configuration;
using Bonsai.Editor;
using System.Diagnostics;

namespace Bonsai
{
    sealed class TypeVisualizerLoader : MarshalByRefObject
    {
        static IEnumerable<TypeVisualizerDescriptor> GetCustomAttributeTypes(Assembly assembly)
        {
            var assemblyAttributes = CustomAttributeData.GetCustomAttributes(assembly);
            var typeVisualizers = assemblyAttributes
                .OfType<TypeVisualizerAttribute>()
                .Select(attribute => new TypeVisualizerDescriptor(attribute));

            var types = assembly.GetTypes();
            for (int i = 0; i < types.Length; i++)
            {
                var type = types[i];
                if (type.IsPublic && !type.IsAbstract && !type.ContainsGenericParameters)
                {
                    var customAttributes = type.GetCustomAttributesData(inherit: true).OfType<TypeVisualizerAttribute>();
                    typeVisualizers = typeVisualizers.Concat(customAttributes.Select(
                        attribute => new TypeVisualizerDescriptor(attribute)
                        {
                            TargetTypeName = type.AssemblyQualifiedName
                        }));
                }
            }

            return typeVisualizers;
        }

        static IEnumerable<TypeVisualizerDescriptor> GetReflectionTypeVisualizerTypes(MetadataLoadContext context, string assemblyName)
        {
            try
            {
                var assembly = context.LoadFromAssemblyName(assemblyName);
                return GetCustomAttributeTypes(assembly);
            }
            catch (FileLoadException ex) { Trace.TraceError("{0}", ex); }
            catch (FileNotFoundException ex) { Trace.TraceError("{0}", ex); }
            catch (BadImageFormatException ex) { Trace.TraceError("{0}", ex); }
            return Enumerable.Empty<TypeVisualizerDescriptor>();
        }

        public static IObservable<TypeVisualizerDescriptor> GetVisualizerTypes(PackageConfiguration configuration)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            var assemblies = configuration.AssemblyReferences.Select(reference => reference.AssemblyName);
            return Observable.Using(
                () => LoaderResource.CreateMetadataLoadContext(configuration),
                context => from assemblyName in assemblies.ToObservable()
                           from typeVisualizer in GetReflectionTypeVisualizerTypes(context, assemblyName)
                           select typeVisualizer);
        }
    }
}
