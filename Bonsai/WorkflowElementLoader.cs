using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.IO;
using Bonsai.Expressions;
using System.Reactive.Linq;
using System.ComponentModel;
using Bonsai.Configuration;
using Bonsai.Editor;
using System.Xml;
using System.Diagnostics;

namespace Bonsai
{
    sealed class WorkflowElementLoader
    {
        const string ExpressionBuilderSuffix = "Builder";

        static bool IsWorkflowElement(Type type, CustomAttributeData[] customAttributes)
        {
            return type.IsMatchSubclassOf(typeof(ExpressionBuilder)) ||
                customAttributes.IsDefined(typeof(CombinatorAttribute)) ||
#pragma warning disable CS0612 // Type or member is obsolete
                customAttributes.IsDefined(typeof(SourceAttribute));
#pragma warning restore CS0612 // Type or member is obsolete
        }

        static bool IsVisibleElement(CustomAttributeData[] customAttributes)
        {
            var visibleAttribute = customAttributes.GetCustomAttributeData(typeof(DesignTimeVisibleAttribute));
            if (visibleAttribute != null)
            {
                return visibleAttribute.ConstructorArguments.Count > 0 &&
                       (bool)visibleAttribute.ConstructorArguments[0].Value;
            }

            return true;
        }

        static string RemoveSuffix(string source, string suffix)
        {
            var suffixStart = source.LastIndexOf(suffix);
            return suffixStart >= 0 ? source.Remove(suffixStart) : source;
        }

        static string GetElementDisplayName(Type type, CustomAttributeData[] customAttributes)
        {
            var displayNameAttribute = customAttributes.GetCustomAttributeData(typeof(DisplayNameAttribute));
            if (displayNameAttribute != null)
            {
                return (string)displayNameAttribute.GetConstructorArgument() ?? string.Empty;
            }

            return type.IsMatchSubclassOf(typeof(ExpressionBuilder))
                ? RemoveSuffix(type.Name, ExpressionBuilderSuffix)
                : type.Name;
        }

        static IEnumerable<WorkflowElementDescriptor> GetWorkflowElements(Assembly assembly)
        {
            var types = assembly.GetTypes();
            for (int i = 0; i < types.Length; i++)
            {
                var type = types[i];
                if (!type.IsPublic || type.IsValueType || type.ContainsGenericParameters || type.IsAbstract)
                {
                    continue;
                }

                var customAttributes = type.GetCustomAttributesData(inherit: true);
                if (IsWorkflowElement(type, customAttributes) &&
                    !customAttributes.IsDefined(typeof(ObsoleteAttribute)) &&
                    IsVisibleElement(customAttributes) && type.GetConstructor(Type.EmptyTypes) != null)
                {
                    var descriptionAttribute = customAttributes.GetCustomAttributeData(typeof(DescriptionAttribute));
                    yield return new WorkflowElementDescriptor
                    {
                        Name = GetElementDisplayName(type, customAttributes),
                        Namespace = type.Namespace,
                        FullyQualifiedName = type.AssemblyQualifiedName,
                        Description = (string)descriptionAttribute?.GetConstructorArgument() ?? string.Empty,
                        ElementTypes = WorkflowElementCategoryConverter.FromType(type, customAttributes).ToArray()
                    };
                }
            }

            const char AssemblySeparator = ':';
            const string BonsaiExtension = ".bonsai";
            var assemblyName = assembly.GetName().Name;
            var embeddedResources = assembly.GetManifestResourceNames();
            for (int i = 0; i < embeddedResources.Length; i++)
            {
                if (Path.GetExtension(embeddedResources[i]) == BonsaiExtension)
                {
                    var description = string.Empty;
                    var name = Path.GetFileNameWithoutExtension(embeddedResources[i]);
                    using var resourceStream = assembly.GetManifestResourceStream(embeddedResources[i]);
                    try
                    {
                        var metadataType = assembly.GetType(name);
                        if (metadataType != null &&
                            metadataType.GetCustomAttributesData(inherit: true)
                                        .IsDefined(typeof(ObsoleteAttribute)))
                        {
                            continue;
                        }

                        using (var reader = XmlReader.Create(resourceStream, new XmlReaderSettings { IgnoreWhitespace = true }))
                        {
                            reader.ReadStartElement(typeof(WorkflowBuilder).Name);
                            if (reader.Name == nameof(WorkflowBuilder.Description))
                            {
                                reader.ReadStartElement();
                                description = reader.Value;
                            }
                        }
                    }
                    catch (SystemException ex)
                    {
                        Trace.TraceError("{0}", ex);
                        continue;
                    }

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

        static IEnumerable<WorkflowElementDescriptor> GetReflectionWorkflowElementTypes(MetadataLoadContext context, string assemblyName)
        {
            try
            {
                var assembly = context.LoadFromAssemblyName(assemblyName);
                return GetWorkflowElements(assembly);
            }
            catch (FileLoadException ex) { Trace.TraceError("{0}", ex); }
            catch (FileNotFoundException ex) { Trace.TraceError("{0}", ex); }
            catch (BadImageFormatException ex) { Trace.TraceError("{0}", ex); }
            return Enumerable.Empty<WorkflowElementDescriptor>();
        }

        public static IObservable<IGrouping<string, WorkflowElementDescriptor>> GetWorkflowElementTypes(PackageConfiguration configuration)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            var assemblies = configuration.AssemblyReferences.Select(reference => reference.AssemblyName);
            return Observable.Using(
                () => LoaderResource.CreateMetadataLoadContext(configuration),
                context => from assemblyName in assemblies.ToObservable()
                           from package in GetReflectionWorkflowElementTypes(context, assemblyName)
                                          .GroupBy(element => element.Namespace)
                           select package);
        }
    }
}
