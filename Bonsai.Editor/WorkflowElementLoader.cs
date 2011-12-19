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

        string[] GetReflectionOnlyWorkflowElementTypes()
        {
            var types = Enumerable.Empty<string>();
            var workflowElementAssembly = Assembly.ReflectionOnlyLoad(typeof(WorkflowElement).Assembly.FullName);
            var workflowElementType = workflowElementAssembly.GetType(typeof(WorkflowElement).FullName);

            var files = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, "*.dll");
            for (int i = 0; i < files.Length; i++)
            {
                try
                {
                    var assembly = Assembly.ReflectionOnlyLoadFrom(files[i]);
                    types = types.Concat(GetSubclassElementTypes(assembly, workflowElementType).Select(type => type.AssemblyQualifiedName));
                }
                catch (BadImageFormatException) { continue; }
            }

            return types.Distinct().ToArray();
        }

        public static bool MatchGenericType(Type type, Type genericType)
        {
            if (!genericType.IsGenericType)
            {
                throw new ArgumentException("Trying to match against a non-generic type.", "genericType");
            }

            while (type != null)
            {
                if (type.IsGenericType && type.GetGenericTypeDefinition() == genericType)
                {
                    return true;
                }

                type = type.BaseType;
            }

            return false;
        }

        public static Type[] GetWorkflowElementTypes()
        {
            var reflectionOnlyDomain = AppDomain.CreateDomain("ReflectionOnly");
            try
            {
                reflectionOnlyDomain.ReflectionOnlyAssemblyResolve += new ResolveEventHandler(reflectionOnlyDomain_ReflectionOnlyAssemblyResolve);
                var loader = (WorkflowElementLoader)reflectionOnlyDomain.CreateInstanceAndUnwrap(typeof(WorkflowElementLoader).Assembly.FullName, typeof(WorkflowElementLoader).FullName);
                var typeNames = loader.GetReflectionOnlyWorkflowElementTypes();

                var types = new Type[typeNames.Length];
                for (int i = 0; i < typeNames.Length; i++)
                {
                    types[i] = Type.GetType(typeNames[i]);
                }
                return types;
            }
            finally { AppDomain.Unload(reflectionOnlyDomain); }
        }

        static Assembly reflectionOnlyDomain_ReflectionOnlyAssemblyResolve(object sender, ResolveEventArgs args)
        {
            return Assembly.ReflectionOnlyLoad(args.Name);
        }
    }
}
