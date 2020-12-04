using Microsoft.CSharp;
using System;
using System.CodeDom;

namespace Bonsai.Editor
{
    static class TypeHelper
    {
        internal static string GetTypeName(Type type)
        {
            return GetTypeName(new CodeTypeReference(type));
        }

        internal static string GetTypeName(string typeName)
        {
            return GetTypeName(new CodeTypeReference(typeName));
        }

        internal static string GetTypeName(CodeTypeReference typeRef)
        {
            using (var provider = new CSharpCodeProvider())
            {
                return provider.GetTypeOutput(typeRef);
            }
        }
    }
}
