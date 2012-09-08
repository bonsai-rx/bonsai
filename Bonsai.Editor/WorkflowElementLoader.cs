using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.IO;
using System.Diagnostics;
using Bonsai.Expressions;
using System.Reactive.Linq;

namespace Bonsai.Editor
{
    public class WorkflowElementLoader : MarshalByRefObject
    {
        IEnumerable<WorkflowElementType> GetSubclassElementTypes(Assembly assembly, Type baseClass)
        {
            Type[] types;

            try { types = assembly.GetTypes(); }
            catch (ReflectionTypeLoadException) { yield break; }

            for (int i = 0; i < types.Length; i++)
            {
                var type = types[i];
                if (!type.IsAbstract && !type.ContainsGenericParameters && type.IsSubclassOf(baseClass))
                {
                    yield return new WorkflowElementType
                    {
                        AssemblyName = type.Assembly.GetName().Name,
                        Type = type.AssemblyQualifiedName
                    };
                }
            }
        }

        WorkflowElementType[] GetReflectionWorkflowElementTypes(string fileName)
        {
            var types = Enumerable.Empty<WorkflowElementType>();
            var loadableElementAssembly = Assembly.Load(typeof(LoadableElement).Assembly.FullName);
            var loadableElementType = loadableElementAssembly.GetType(typeof(LoadableElement).FullName);
            var expressionBuilderType = loadableElementAssembly.GetType(typeof(ExpressionBuilder).FullName);

            try
            {
                var assembly = Assembly.LoadFrom(fileName);
                types = types.Concat(GetSubclassElementTypes(assembly, loadableElementType))
                             .Concat(GetSubclassElementTypes(assembly, expressionBuilderType));
            }
            catch (FileLoadException) { }
            catch (FileNotFoundException) { }
            catch (BadImageFormatException) { }

            return types.Distinct().ToArray();
        }

        [Serializable]
        [DebuggerDisplay("Type = {Type}, AssemblyName = {AssemblyName}")]
        struct WorkflowElementType
        {
            public string AssemblyName { get; set; }

            public string Type { get; set; }
        }

        class LoaderResource : IDisposable
        {
            AppDomain reflectionDomain;

            public LoaderResource()
            {
                AppDomainSetup setup = AppDomain.CurrentDomain.SetupInformation;
                setup.LoaderOptimization = LoaderOptimization.MultiDomainHost;
                reflectionDomain = AppDomain.CreateDomain("ReflectionOnly", AppDomain.CurrentDomain.Evidence, setup);
                Loader = (WorkflowElementLoader)reflectionDomain.CreateInstanceAndUnwrap(typeof(WorkflowElementLoader).Assembly.FullName, typeof(WorkflowElementLoader).FullName);
            }

            public WorkflowElementLoader Loader { get; private set; }

            public void Dispose()
            {
                AppDomain.Unload(reflectionDomain);
            }
        }

        class WorkflowElementGroup : IGrouping<string, Type>
        {
            IEnumerable<Type> elementTypes;

            public WorkflowElementGroup(string key, IEnumerable<WorkflowElementType> types)
            {
                Key = key;
                elementTypes = from elementType in types
                               let type = Type.GetType(elementType.Type)
                               where type != null
                               select type;
            }

            public string Key { get; private set; }

            public IEnumerator<Type> GetEnumerator()
            {
                return elementTypes.GetEnumerator();
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return elementTypes.GetEnumerator();
            }
        }

        public static IObservable<IGrouping<string, Type>> GetWorkflowElementTypes()
        {
            var files = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, "*.dll");
            return Observable.Using(
                () => new LoaderResource(),
                resource => from fileName in files.ToObservable()
                            let assemblyTypeNames = resource.Loader.GetReflectionWorkflowElementTypes(fileName)
                            select new WorkflowElementGroup(Path.GetFileNameWithoutExtension(fileName), assemblyTypeNames));
        }
    }
}
