using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Bonsai.Editor
{
    static class TypeDefinitionProvider
    {
        static CodeTypeReference GetTypeReference(Type type, HashSet<string> importNamespaces)
        {
            var baseType = type.IsArray || type.IsPointer || type.IsByRef ? type.GetElementType() : type;
            if (baseType.IsPrimitive || baseType == typeof(string) || baseType == typeof(object))
            {
                return new CodeTypeReference(type);
            }

            importNamespaces.Add(type.Namespace);
            var reference = new CodeTypeReference(type.Name);
            if (type.IsArray) reference.ArrayRank = type.GetArrayRank();
            if (type.IsGenericType)
            {
                var typeParameters = type.GetGenericArguments();
                reference.TypeArguments.AddRange(Array.ConvertAll(typeParameters, parameter => GetTypeReference(parameter, importNamespaces)));
            }
            return reference;
        }

        static CodeAttributeDeclaration GetAttributeDeclaration(CustomAttributeData attribute, HashSet<string> importNamespaces)
        {
            importNamespaces.Add(attribute.AttributeType.Namespace);
            var attributeName = attribute.AttributeType.Name;
            var suffix = attributeName.LastIndexOf(nameof(Attribute));
            attributeName = suffix >= 0 ? attributeName.Substring(0, suffix) : attributeName;
            var reference = new CodeTypeReference(attributeName);
            var declaration = new CodeAttributeDeclaration(reference);
            foreach (var argument in attribute.ConstructorArguments)
            {
                CodeExpression value;
                if (argument.ArgumentType == typeof(Type))
                {
                    var type = (Type)argument.Value;
                    value = new CodeTypeOfExpression(GetTypeReference(type, importNamespaces));
                }
                else value = new CodePrimitiveExpression(argument.Value);
                declaration.Arguments.Add(new CodeAttributeArgument(value));
            }
            foreach (var argument in attribute.NamedArguments)
            {
                CodeExpression value;
                if (argument.TypedValue.ArgumentType == typeof(Type))
                {
                    var type = (Type)argument.TypedValue.Value;
                    value = new CodeTypeOfExpression(GetTypeReference(type, importNamespaces));
                }
                else value = new CodePrimitiveExpression(argument.TypedValue.Value);
                declaration.Arguments.Add(new CodeAttributeArgument(argument.MemberName, value));
            }
            return declaration;
        }

        static CodeTypeMember GetPropertyDeclaration(PropertyInfo property, HashSet<string> importNamespaces, HashSet<MethodInfo> getterSetters)
        {
            var declaration = new CodeMemberProperty();
            declaration.Name = property.Name;
            declaration.Attributes = MemberAttributes.Public | MemberAttributes.Final;
            var attributes = property.GetCustomAttributesData()
                .Select(a => GetAttributeDeclaration(a, importNamespaces))
                .ToArray();
            declaration.CustomAttributes.AddRange(attributes);

            var getter = property.GetGetMethod();
            if (getter != null)
            {
                declaration.HasGet = true;
                getterSetters.Add(getter);
            }
            else declaration.HasGet = false;

            var setter = property.GetSetMethod();
            if (setter != null)
            {
                declaration.HasSet = true;
                getterSetters.Add(setter);
            }
            else declaration.HasSet = false;

            declaration.Type = GetTypeReference(property.PropertyType, importNamespaces);
            return declaration;
        }

        static CodeTypeMember GetMethodDeclaration(MethodInfo method, HashSet<string> importNamespaces)
        {
            var declaration = new CodeMemberMethod();
            declaration.Name = method.Name;
            declaration.Attributes = MemberAttributes.Public | MemberAttributes.Final;
            declaration.ReturnType = GetTypeReference(method.ReturnType, importNamespaces);
            if (method.IsGenericMethod)
            {
                var typeParameters = method.GetGenericArguments();
                declaration.TypeParameters.AddRange(Array.ConvertAll(typeParameters, parameter => new CodeTypeParameter(parameter.Name)));
            }

            declaration.Parameters.AddRange(Array.ConvertAll(method.GetParameters(), parameter =>
            {
                var declaration = new CodeParameterDeclarationExpression();
                declaration.Name = parameter.Name;
                declaration.Type = GetTypeReference(parameter.ParameterType, importNamespaces);
                declaration.Direction = parameter.ParameterType.IsByRef
                    ? (parameter.IsOut ? FieldDirection.Out : FieldDirection.Ref)
                    : FieldDirection.In;
                return declaration;
            }));
            return declaration;
        }

        static CodeTypeDeclaration GetTypeDeclaration(Type type, HashSet<string> importNamespaces)
        {
            var getterSetters = new HashSet<MethodInfo>();
            var declaration = new CodeTypeDeclaration(type.Name);
            var attributes = type.GetCustomAttributesData()
                .Select(a => GetAttributeDeclaration(a, importNamespaces))
                .ToArray();

            if (type.BaseType != null && type.BaseType != typeof(object))
            {
                declaration.BaseTypes.Add(GetTypeReference(type.BaseType, importNamespaces));
            }

            var interfaces = type.GetInterfaces();
            if (interfaces.Length > 0)
            {
                declaration.BaseTypes.AddRange(Array.ConvertAll(interfaces, i => GetTypeReference(i, importNamespaces)));
            }

            var properties = type.GetProperties().Select(p => GetPropertyDeclaration(p, importNamespaces, getterSetters));
            var methods = type.GetMethods().Except(getterSetters).Select(m => GetMethodDeclaration(m, importNamespaces));
            var members = properties.Concat(methods).Where(declaration => declaration != null).ToArray();
            declaration.CustomAttributes.AddRange(attributes);
            declaration.Members.AddRange(members);
            return declaration;
        }

        public static CodeCompileUnit GetTypeDefinition(Type type)
        {
            var result = new CodeCompileUnit();
            var globalNamespace = new CodeNamespace();
            var importNamespaces = new HashSet<string>();
            var typeNamespace = new CodeNamespace(type.Namespace);
            var typeDeclaration = GetTypeDeclaration(type, importNamespaces);
            typeNamespace.Types.Add(typeDeclaration);
            var importDeclarations = importNamespaces.Select(name => new CodeNamespaceImport(name)).ToArray();
            globalNamespace.Imports.AddRange(importDeclarations);
            result.Namespaces.Add(globalNamespace);
            result.Namespaces.Add(typeNamespace);
            return result;
        }
    }
}
