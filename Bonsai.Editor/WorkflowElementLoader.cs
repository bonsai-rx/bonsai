using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.IO;
using System.Diagnostics;

namespace Bonsai.Editor
{
    public class WorkflowElementLoader : MarshalByRefObject
    {
        IEnumerable<Type> GetSubclassElementTypes(Assembly assembly, Type baseClass)
        {
            Type[] types;

            try { types = assembly.GetTypes(); }
            catch (ReflectionTypeLoadException) { yield break; }

            for (int i = 0; i < types.Length; i++)
            {
                if (!types[i].IsAbstract && !types[i].ContainsGenericParameters && types[i].IsSubclassOf(baseClass))
                {
                    yield return types[i];
                }
            }
        }

        WorkflowElementType[] GetReflectionWorkflowElementTypes()
        {
            var types = Enumerable.Empty<WorkflowElementType>();
            var loadableElementAssembly = Assembly.Load(typeof(LoadableElement).Assembly.FullName);
            var loadableElementType = loadableElementAssembly.GetType(typeof(LoadableElement).FullName);

            var files = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, "*.dll");
            for (int i = 0; i < files.Length; i++)
            {
                try
                {
                    var assembly = Assembly.LoadFrom(files[i]);
                    types = types.Concat(GetSubclassElementTypes(assembly, loadableElementType).Select(type => new WorkflowElementType
                    {
                        AssemblyName = type.Assembly.GetName().Name,
                        Type = type.AssemblyQualifiedName
                    }));
                }
                catch (FileLoadException) { continue; }
                catch (FileNotFoundException) { continue; }
                catch (BadImageFormatException) { continue; }
            }

            return types.Distinct().ToArray();
        }

        [Serializable]
        [DebuggerDisplay("Type = {Type}, AssemblyName = {AssemblyName}")]
        struct WorkflowElementType
        {
            public string AssemblyName { get; set; }

            public string Type { get; set; }
        }

        public static IEnumerable<IGrouping<string, Type>> GetWorkflowElementTypes()
        {
            var reflectionDomain = AppDomain.CreateDomain("ReflectionOnly");
            try
            {
                var loader = (WorkflowElementLoader)reflectionDomain.CreateInstanceAndUnwrap(typeof(WorkflowElementLoader).Assembly.FullName, typeof(WorkflowElementLoader).FullName);
                var assemblyTypeNames = loader.GetReflectionWorkflowElementTypes();

                return from elementType in assemblyTypeNames
                       let type = Type.GetType(elementType.Type)
                       where type != null
                       group type by elementType.AssemblyName;
            }
            finally { AppDomain.Unload(reflectionDomain); }
        }
    }
}
