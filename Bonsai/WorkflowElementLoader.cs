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
using Bonsai.Editor;

namespace Bonsai
{
    sealed class WorkflowElementLoader : MarshalByRefObject
    {
        Type sourceAttributeType;
        Type combinatorAttributeType;
        Type expressionBuilderType;

        public WorkflowElementLoader(PackageConfiguration configuration)
        {
            ConfigurationHelper.SetAssemblyResolve(configuration);
            InitializeReflectionTypes();
        }

        void InitializeReflectionTypes()
        {
            var expressionBuilderAssembly = Assembly.Load(typeof(ExpressionBuilder).Assembly.FullName);
            sourceAttributeType = expressionBuilderAssembly.GetType(typeof(SourceAttribute).FullName);
            combinatorAttributeType = expressionBuilderAssembly.GetType(typeof(CombinatorAttribute).FullName);
            expressionBuilderType = expressionBuilderAssembly.GetType(typeof(ExpressionBuilder).FullName);
        }

        bool IsWorkflowElement(Type type)
        {
            return type.IsSubclassOf(expressionBuilderType) ||
                type.IsDefined(combinatorAttributeType, true) ||
                type.IsDefined(sourceAttributeType, true);
        }

        IEnumerable<WorkflowElementDescriptor> GetWorkflowElements(Assembly assembly)
        {
            Type[] types;

            try { types = assembly.GetTypes(); }
            catch (ReflectionTypeLoadException) { yield break; }

            for (int i = 0; i < types.Length; i++)
            {
                var type = types[i];
                if (type.IsPublic && !type.IsAbstract && !type.ContainsGenericParameters &&
                    IsWorkflowElement(type) && !type.IsDefined(typeof(ObsoleteAttribute)))
                {
                    var descriptionAttribute = (DescriptionAttribute)TypeDescriptor.GetAttributes(type)[typeof(DescriptionAttribute)];
                    yield return new WorkflowElementDescriptor
                    {
                        Name = ExpressionBuilder.GetElementDisplayName(type),
                        Namespace = type.Namespace,
                        FullyQualifiedName = type.AssemblyQualifiedName,
                        Description = descriptionAttribute.Description,
                        ElementTypes = WorkflowElementCategoryConverter.FromType(type).ToArray()
                    };
                }
            }
        }

        WorkflowElementDescriptor[] GetReflectionWorkflowElementTypes(string assemblyRef)
        {
            var types = Enumerable.Empty<WorkflowElementDescriptor>();
            try
            {
                var assembly = Assembly.Load(assemblyRef);
                types = types.Concat(GetWorkflowElements(assembly));
            }
            catch (FileLoadException) { }
            catch (FileNotFoundException) { }
            catch (BadImageFormatException) { }

            return types.Distinct().ToArray();
        }

        class LoaderResource : IDisposable
        {
            AppDomain reflectionDomain;

            public LoaderResource(PackageConfiguration configuration)
            {
                var currentEvidence = AppDomain.CurrentDomain.Evidence;
                var setupInfo = AppDomain.CurrentDomain.SetupInformation;
                reflectionDomain = AppDomain.CreateDomain("ReflectionOnly", currentEvidence, setupInfo);
                Loader = (WorkflowElementLoader)reflectionDomain.CreateInstanceAndUnwrap(
                    typeof(WorkflowElementLoader).Assembly.FullName,
                    typeof(WorkflowElementLoader).FullName,
                    false, (BindingFlags)0, null,
                    new[] { configuration }, null, null);
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

        public static IObservable<IGrouping<string, WorkflowElementDescriptor>> GetWorkflowElementTypes(PackageConfiguration configuration)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException("configuration");
            }

            var assemblies = configuration.AssemblyReferences.Select(reference => reference.AssemblyName);
            return Observable.Using(
                () => new LoaderResource(configuration),
                resource => from assemblyRef in assemblies.ToObservable()
                            from package in resource.Loader
                                .GetReflectionWorkflowElementTypes(assemblyRef)
                                .GroupBy(element => element.Namespace)
                            select package);
        }
    }
}
