using Microsoft.CSharp;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Bonsai.Editor
{
    class TypeDefinitionCodeProvider : CSharpCodeProvider
    {
        public override void GenerateCodeFromCompileUnit(CodeCompileUnit compileUnit, TextWriter writer, CodeGeneratorOptions options)
        {
            using (var indentedWriter = new IndentedTextWriter(writer))
            {
                foreach (CodeNamespace codeNamespace in compileUnit.Namespaces)
                {
                    GenerateCodeFromNamespace(codeNamespace, indentedWriter);
                    indentedWriter.WriteLine();
                }
            }
        }

        void GenerateCodeFromNamespace(CodeNamespace codeNamespace, IndentedTextWriter writer)
        {
            var hasDeclaration = !string.IsNullOrEmpty(codeNamespace.Name);
            if (hasDeclaration)
            {
                writer.WriteLine($"namespace {codeNamespace.Name}");
                writer.WriteLine("{");
                writer.Indent++;
            }

            foreach (var import in codeNamespace.Imports.Cast<CodeNamespaceImport>().OrderBy(ns => ns.Namespace))
            {
                WriteNamespaceImport(import, writer);
            }

            foreach (var type in codeNamespace.Types.Cast<CodeTypeDeclaration>())
            {
                WriteTypeDeclaration(type, writer);
            }

            if (hasDeclaration)
            {
                writer.Indent--;
                writer.WriteLine("}");
            }
        }

        void WriteNamespaceImport(CodeNamespaceImport import, IndentedTextWriter writer)
        {
            writer.WriteLine($"using {import.Namespace};");
        }

        void WriteTypeDeclaration(CodeTypeDeclaration type, IndentedTextWriter writer)
        {
            if (string.IsNullOrEmpty(type.Name)) return;
            foreach (var attribute in type.CustomAttributes.Cast<CodeAttributeDeclaration>())
            {
                WriteAttributeDeclaration(attribute, writer);
            }

            var typeKeyword = type.IsClass ? "class" : "struct";
            var baseTypes = string.Empty;
            if (type.BaseTypes.Count > 0)
            {
                baseTypes = $" : {string.Join(", ", type.BaseTypes.Cast<CodeTypeReference>().Select(baseType => GetTypeOutput(baseType)))}";
            }

            writer.Write($"public {typeKeyword} {type.Name}");
            WriteTypeParameters(type.TypeParameters, writer, out string constraints);
            if (!string.IsNullOrEmpty(constraints)) baseTypes += constraints;
            writer.WriteLine(baseTypes);
            writer.WriteLine("{");
            writer.Indent++;

            var properties = new List<CodeMemberProperty>();
            var methods = new List<CodeMemberMethod>();
            foreach (var member in type.Members.Cast<CodeTypeMember>())
            {
                if (member is CodeMemberProperty property) properties.Add(property);
                else if (member is CodeMemberMethod method)
                {
                    methods.Add(method);
                }
            }

            foreach (var property in properties)
            {
                WritePropertyDeclaration(property, writer);
                writer.WriteLine();
            }

            foreach (var method in methods)
            {
                WriteMethodDeclaration(method, writer);
            }

            writer.Indent--;
            writer.WriteLine("}");
        }

        void WriteAttributeDeclaration(CodeAttributeDeclaration attribute, IndentedTextWriter writer)
        {
            if (string.IsNullOrEmpty(attribute.Name)) return;
            var arguments = string.Empty;
            if (attribute.Arguments.Count > 0)
            {
                var argumentValues = attribute.Arguments.Cast<CodeAttributeArgument>().Select(argument =>
                {
                    if (argument.Value is CodePrimitiveExpression primitive)
                    {
                        if (primitive.Value == null) return "null";
                        else if (primitive.Value is string text) return $"\"{text}\"";
                        else return primitive.Value.ToString();
                    }
                    else if (argument.Value is CodeTypeOfExpression typeOf) return $"typeof({GetTypeOutput(typeOf.Type)})";
                    else if (argument.Value is CodeSnippetExpression snippet) return snippet.Value;
                    return string.Empty;
                });
                arguments = $"({string.Join(", ", argumentValues)})";
            }
            writer.WriteLine($"[{attribute.Name}{arguments}]");
        }

        void WritePropertyDeclaration(CodeMemberProperty property, IndentedTextWriter writer)
        {
            if (string.IsNullOrEmpty(property.Name) || !property.HasGet) return;
            foreach (var attribute in property.CustomAttributes.Cast<CodeAttributeDeclaration>())
            {
                WriteAttributeDeclaration(attribute, writer);
            }

            var getterSetter = property.HasSet ? "{ get; set; }" : "{ get; }";
            writer.WriteLine($"public {GetTypeOutput(property.Type)} {property.Name} {getterSetter}");
        }

        void WriteTypeParameters(CodeTypeParameterCollection typeParameters, IndentedTextWriter writer, out string typeConstraints)
        {
            var constraints = string.Empty;
            if (typeParameters.Count > 0)
            {
                var parameterDeclarations = typeParameters.Cast<CodeTypeParameter>().Select(p =>
                {
                    if (p.Constraints.Count > 0 || p.HasConstructorConstraint)
                    {
                        var parameterConstraints = new List<string>(p.Constraints.Count);
                        parameterConstraints.AddRange(p.Constraints.Cast<CodeTypeReference>().Select(type =>
                        {
                            if (type.BaseType == "System.ValueType") return "struct";
                            else if (type.BaseType == "System.Object") return "class";
                            else return GetTypeOutput(type);
                        }));
                        if (p.HasConstructorConstraint) parameterConstraints.Add("new()");
                        constraints += $" where {p.Name} : {string.Join(", ", parameterConstraints)}";
                    }
                    return p.Name;
                });
                writer.Write($"<{string.Join(", ", parameterDeclarations)}>");
            }
            typeConstraints = constraints;
        }

        void WriteMethodDeclaration(CodeMemberMethod method, IndentedTextWriter writer)
        {
            if (string.IsNullOrEmpty(method.Name)) return;
            foreach (var attribute in method.CustomAttributes.Cast<CodeAttributeDeclaration>())
            {
                WriteAttributeDeclaration(attribute, writer);
            }

            string modifiers;
            var prefix = false;
            if (method.Attributes.HasFlag(MemberAttributes.Final)) modifiers = string.Empty;
            else if (method.Attributes.HasFlag(MemberAttributes.Override)) modifiers = "override ";
            else modifiers = "virtual ";
            writer.Write($"public {modifiers}{GetTypeOutput(method.ReturnType)} {method.Name}");
            WriteTypeParameters(method.TypeParameters, writer, out string constraints);
            writer.Write("(");
            foreach (var parameter in method.Parameters.Cast<CodeParameterDeclarationExpression>())
            {
                if (prefix) writer.Write(", ");
                WriteParameterDeclaration(parameter, writer);
                prefix = true;
            }
            writer.WriteLine($"){constraints};");
        }

        void WriteParameterDeclaration(CodeParameterDeclarationExpression parameter, IndentedTextWriter writer)
        {
            string direction;
            switch (parameter.Direction)
            {
                case FieldDirection.Out: direction = "out "; break;
                case FieldDirection.Ref: direction = "ref "; break;
                case FieldDirection.In:
                default: direction = string.Empty; break;
            }
            writer.Write($"{direction}{GetTypeOutput(parameter.Type)} {parameter.Name}");
        }
    }
}
