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
        public WorkflowElementLoader(PackageConfiguration configuration)
        {
            ConfigurationHelper.SetAssemblyResolve(configuration);
        }

        static bool IsWorkflowElement(Type type)
        {
            return type.IsSubclassOf(typeof(ExpressionBuilder)) ||
                type.IsDefined(typeof(CombinatorAttribute), true) ||
                type.IsDefined(typeof(SourceAttribute), true);
        }

        static IEnumerable<WorkflowElementDescriptor> GetWorkflowElements(Assembly assembly)
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

        public static IObservable<IGrouping<string, WorkflowElementDescriptor>> GetWorkflowElementTypes(PackageConfiguration configuration)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException("configuration");
            }

            var assemblies = configuration.AssemblyReferences.Select(reference => reference.AssemblyName);
            return Observable.Using(
                () => new LoaderResource<WorkflowElementLoader>(configuration),
                resource => from assemblyRef in assemblies.ToObservable()
                            from package in resource.Loader
                                .GetReflectionWorkflowElementTypes(assemblyRef)
                                .GroupBy(element => element.Namespace)
                            select package);
        }
    }
}
