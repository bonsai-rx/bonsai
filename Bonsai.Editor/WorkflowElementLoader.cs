using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.IO;

namespace Bonsai.Editor
{
    public class WorkflowElementLoader : MarshalByRefObject
    {
        IEnumerable<Type> GetSubclassElementTypes(Assembly assembly, Type baseClass)
        {
            var types = assembly.GetTypes();
            for (int i = 0; i < types.Length; i++)
            {
                if (!types[i].IsAbstract && !types[i].ContainsGenericParameters && types[i].IsSubclassOf(baseClass))
                {
                    yield return types[i];
                }
            }
        }

        string[] GetReflectionWorkflowElementTypes()
        {
            var types = Enumerable.Empty<string>();
            var loadableElementAssembly = Assembly.Load(typeof(LoadableElement).Assembly.FullName);
            var loadableElementType = loadableElementAssembly.GetType(typeof(LoadableElement).FullName);

            var files = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, "*.dll");
            for (int i = 0; i < files.Length; i++)
            {
                try
                {
                    var assembly = Assembly.LoadFrom(files[i]);
                    types = types.Concat(GetSubclassElementTypes(assembly, loadableElementType).Select(type => type.AssemblyQualifiedName));
                }
                catch (BadImageFormatException) { continue; }
            }

            return types.Distinct().ToArray();
        }

        public static Type[] GetWorkflowElementTypes()
        {
            var reflectionDomain = AppDomain.CreateDomain("ReflectionOnly");
            try
            {
                var loader = (WorkflowElementLoader)reflectionDomain.CreateInstanceAndUnwrap(typeof(WorkflowElementLoader).Assembly.FullName, typeof(WorkflowElementLoader).FullName);
                var typeNames = loader.GetReflectionWorkflowElementTypes();

                var types = new Type[typeNames.Length];
                for (int i = 0; i < typeNames.Length; i++)
                {
                    types[i] = Type.GetType(typeNames[i]);
                }
                return types;
            }
            finally { AppDomain.Unload(reflectionDomain); }
        }
    }
}
