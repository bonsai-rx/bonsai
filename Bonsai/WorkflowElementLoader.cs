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
using System.Xml;

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

        static bool IsVisibleElement(Type type)
        {
            var visibleAttribute = type.GetCustomAttribute<DesignTimeVisibleAttribute>() ?? DesignTimeVisibleAttribute.Default;
            return visibleAttribute.Visible;
        }

        static IEnumerable<WorkflowElementDescriptor> GetWorkflowElements(Assembly assembly)
        {
            Type[] types;

            try { types = assembly.GetTypes(); }
            catch (ReflectionTypeLoadException) { yield break; }

            for (int i = 0; i < types.Length; i++)
            {
                var type = types[i];
                if (type.IsPublic && !type.IsValueType && !type.ContainsGenericParameters &&
                    !type.IsAbstract && IsWorkflowElement(type) && !type.IsDefined(typeof(ObsoleteAttribute)) &&
                    IsVisibleElement(type) && type.GetConstructor(Type.EmptyTypes) != null)
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

            const char AssemblySeparator = ':';
            const string BonsaiExtension = ".bonsai";
            var embeddedResources = assembly.GetManifestResourceNames();
            for (int i = 0; i < embeddedResources.Length; i++)
            {
                if (Path.GetExtension(embeddedResources[i]) == BonsaiExtension)
                {
                    var description = string.Empty;
                    var resourceStream = assembly.GetManifestResourceStream(embeddedResources[i]);
                    try
                    {
                        using (var reader = XmlReader.Create(resourceStream, new XmlReaderSettings { IgnoreWhitespace = true }))
                        {
                            reader.ReadStartElement(typeof(WorkflowBuilder).Name);
                            if (reader.Name == "Description")
                            {
                                reader.ReadStartElement();
                                description = reader.Value;
                            }
                        }
                    }
                    catch (SystemException) { continue; }

                    var assemblyName = assembly.GetName().Name;
                    var name = Path.GetFileNameWithoutExtension(embeddedResources[i]);
                    var nameSeparator = name.LastIndexOf(ExpressionHelper.MemberSeparator);
                    yield return new WorkflowElementDescriptor
                    {
                        Name = nameSeparator >= 0 ? name.Substring(nameSeparator + 1) : name,
                        Namespace = nameSeparator >= 0 ? name.Substring(0, nameSeparator) : name,
                        FullyQualifiedName = string.Concat(assemblyName, AssemblySeparator, embeddedResources[i].Substring(assemblyName.Length + 1)),
                        Description = description,
                        ElementTypes = new[] { ~ElementCategory.Workflow }
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
