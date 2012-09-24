using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.IO;
using System.Diagnostics;
using Bonsai.Expressions;
using System.Reactive.Linq;
using System.ComponentModel;
using Bonsai.Configuration;

namespace Bonsai.Editor
{
    sealed class WorkflowElementLoader : MarshalByRefObject
    {
        const string ExpressionBuilderSuffix = "Builder";

        Type loadableElementType;
        Type expressionBuilderType;

        public WorkflowElementLoader()
        {
            ConfigurationHelper.SetAssemblyResolve(Environment.CurrentDirectory);
            var loadableElementAssembly = Assembly.Load(typeof(LoadableElement).Assembly.FullName);
            loadableElementType = loadableElementAssembly.GetType(typeof(LoadableElement).FullName);
            expressionBuilderType = loadableElementAssembly.GetType(typeof(ExpressionBuilder).FullName);
        }

        //TODO: Remove duplicate method from ExpressionBuilderTypeConverter.cs
        string RemoveSuffix(string source, string suffix)
        {
            var suffixStart = source.LastIndexOf(suffix);
            return suffixStart >= 0 ? source.Remove(suffixStart) : source;
        }

        string GetElementDisplayName(Type type)
        {
            var displayNameAttribute = (DisplayNameAttribute)TypeDescriptor.GetAttributes(type)[typeof(DisplayNameAttribute)];
            if (displayNameAttribute != null && !string.IsNullOrEmpty(displayNameAttribute.DisplayName))
            {
                return displayNameAttribute.DisplayName;
            }
            else return type.IsSubclassOf(typeof(ExpressionBuilder)) ? RemoveSuffix(type.Name, ExpressionBuilderSuffix) : type.Name;
        }

        IEnumerable<WorkflowElementDescriptor> GetSubclassElementTypes(Assembly assembly, Type baseClass)
        {
            Type[] types;

            try { types = assembly.GetTypes(); }
            catch (ReflectionTypeLoadException) { yield break; }

            for (int i = 0; i < types.Length; i++)
            {
                var type = types[i];
                if (!type.IsAbstract && !type.ContainsGenericParameters && type.IsSubclassOf(baseClass))
                {
                    var descriptionAttribute = (DescriptionAttribute)TypeDescriptor.GetAttributes(type)[typeof(DescriptionAttribute)];
                    yield return new WorkflowElementDescriptor
                    {
                        Name = GetElementDisplayName(type),
                        AssemblyName = type.Assembly.GetName().Name,
                        AssemblyQualifiedName = type.AssemblyQualifiedName,
                        Description = descriptionAttribute.Description,
                        ElementTypes = WorkflowElementTypeConverter.FromType(type).ToArray()
                    };
                }
            }
        }

        WorkflowElementDescriptor[] GetReflectionWorkflowElementTypes(string fileName)
        {
            var types = Enumerable.Empty<WorkflowElementDescriptor>();
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

        class WorkflowElementGroup : IGrouping<string, WorkflowElementDescriptor>
        {
            IEnumerable<WorkflowElementDescriptor> elementTypes;

            public WorkflowElementGroup(string key, IEnumerable<WorkflowElementDescriptor> types)
            {
                Key = key;
                elementTypes = types;
            }

            public string Key { get; private set; }

            public IEnumerator<WorkflowElementDescriptor> GetEnumerator()
            {
                return elementTypes.GetEnumerator();
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return elementTypes.GetEnumerator();
            }
        }

        public static IObservable<IGrouping<string, WorkflowElementDescriptor>> GetWorkflowElementTypes()
        {
            var files = ConfigurationHelper.GetPackageFiles();
            return Observable.Using(
                () => new LoaderResource(),
                resource => from fileName in files.ToObservable()
                            let assemblyTypeNames = resource.Loader.GetReflectionWorkflowElementTypes(fileName)
                            select new WorkflowElementGroup(Path.GetFileNameWithoutExtension(fileName), assemblyTypeNames));
        }
    }

    [Serializable]
    [DebuggerDisplay("Type = {Type}, AssemblyName = {AssemblyName}")]
    struct WorkflowElementDescriptor
    {
        public string Name { get; set; }

        public string AssemblyName { get; set; }

        public string AssemblyQualifiedName { get; set; }

        public string Description { get; set; }

        public WorkflowElementType[] ElementTypes { get; set; }
    }
}
