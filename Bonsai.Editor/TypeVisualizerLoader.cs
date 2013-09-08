using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.IO;
using System.Reactive.Linq;
using Bonsai.Configuration;

namespace Bonsai.Editor
{
    sealed class TypeVisualizerLoader : MarshalByRefObject
    {
        Type typeVisualizerAttributeType;

        public TypeVisualizerLoader()
        {
            ConfigurationHelper.SetAssemblyResolve();
            InitializeReflectionTypes();
        }

        void InitializeReflectionTypes()
        {
            var typeVisualizerAttributeAssembly = Assembly.Load(typeof(TypeVisualizerAttribute).Assembly.FullName);
            typeVisualizerAttributeType = typeVisualizerAttributeAssembly.GetType(typeof(TypeVisualizerAttribute).FullName);
        }

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
                var type = types[i];
                if (type.IsPublic && !type.IsAbstract && !type.ContainsGenericParameters)
                {
                    var visualizerAttributes = Array.ConvertAll(type.GetCustomAttributes(attributeType, true), attribute =>
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

        TypeVisualizerInfo[] GetReflectionTypeVisualizerAttributes(string assemblyRef)
        {
            var typeVisualizers = Enumerable.Empty<TypeVisualizerAttribute>();
            try
            {
                var assembly = Assembly.Load(assemblyRef);
                var visualizerAttributes = assembly.GetCustomAttributes(typeVisualizerAttributeType, true).Cast<TypeVisualizerAttribute>();
                typeVisualizers = typeVisualizers.Concat(visualizerAttributes);
                typeVisualizers = typeVisualizers.Concat(GetCustomAttributeTypes(assembly, typeVisualizerAttributeType));
            }
            catch (FileLoadException) { }
            catch (FileNotFoundException) { }
            catch (BadImageFormatException) { }

            return typeVisualizers.Distinct().Select(data => new TypeVisualizerInfo(data)).ToArray();
        }

        class LoaderResource : IDisposable
        {
            AppDomain reflectionDomain;

            public LoaderResource()
            {
                AppDomainSetup setup = AppDomain.CurrentDomain.SetupInformation;
                setup.LoaderOptimization = LoaderOptimization.MultiDomainHost;
                reflectionDomain = AppDomain.CreateDomain("ReflectionOnly", AppDomain.CurrentDomain.Evidence, setup);
                Loader = (TypeVisualizerLoader)reflectionDomain.CreateInstanceAndUnwrap(
                    typeof(TypeVisualizerLoader).Assembly.FullName,
                    typeof(TypeVisualizerLoader).FullName);
            }

            public TypeVisualizerLoader Loader { get; private set; }

            public void Dispose()
            {
                AppDomain.Unload(reflectionDomain);
            }
        }

        public static IObservable<Tuple<Type, Type>> GetTypeVisualizerDictionary()
        {
            var configuration = ConfigurationHelper.Load();
            var assemblies = configuration.Packages.Select(reference => reference.Name);
            return Observable.Using(
                () => new LoaderResource(),
                resource => from assemblyRef in assemblies.ToObservable()
                            let typeVisualizers = resource.Loader.GetReflectionTypeVisualizerAttributes(assemblyRef)
                            from typeVisualizer in typeVisualizers
                            let targetType = Type.GetType(typeVisualizer.TargetTypeName)
                            let visualizerType = Type.GetType(typeVisualizer.VisualizerTypeName)
                            where targetType != null && visualizerType != null
                            select Tuple.Create(targetType, visualizerType));
        }
    }
}
