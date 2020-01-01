using Bonsai.Dag;
using Bonsai.Design;
using Bonsai.Editor.GraphView;
using Bonsai.Editor.Properties;
using Bonsai.Expressions;
using Microsoft.CSharp;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Bonsai.Editor.Scripting
{
    class ScriptComponentEditor : WorkflowComponentEditor
    {
        const string DefaultScriptName = "Script";
        const string ScriptFilter = "C# Files (*.cs)|*.cs|All Files (*.*)|*.*";
        static readonly string[] IgnoreAssemblyReferences = new[] { "mscorlib.dll", "System.dll", "System.Core.dll", "System.Reactive.Linq.dll" };
        static ModuleBuilder moduleBuilder;
        static ConstructorInfo emptyExpression;

        static CodeTypeReference CreateTypeReference(Type type)
        {
            var typeName = (type.IsPrimitive || type == typeof(string)) ? type.FullName : type.Name;
            var reference = new CodeTypeReference(typeName);
            if (type.IsArray)
            {
                reference.ArrayElementType = CreateTypeReference(type.GetElementType());
                reference.ArrayRank = type.GetArrayRank();
            }
            else if (type.IsGenericType)
            {
                foreach (var argument in type.GetGenericArguments())
                {
                    reference.TypeArguments.Add(CreateTypeReference(argument));
                }
            }
            return reference;
        }

        static void CollectNamespaces(Type type, HashSet<string> namespaces)
        {
            if (!string.IsNullOrEmpty(type.Namespace))
            {
                namespaces.Add(type.Namespace);
            }

            if (type.IsGenericType)
            {
                var genericArguments = type.GetGenericArguments();
                for (int i = 0; i < genericArguments.Length; i++)
                {
                    CollectNamespaces(genericArguments[i], namespaces);
                }
            }
            else if (type.IsArray)
            {
                CollectNamespaces(type.GetElementType(), namespaces);
            }
        }

        static void CollectAssemblyReferences(Type type, HashSet<string> assemblyReferences)
        {
            var assemblyName = type.Assembly.GetName().Name;
            assemblyReferences.Add(assemblyName);
            if (type.IsGenericType)
            {
                var genericArguments = type.GetGenericArguments();
                for (int i = 0; i < genericArguments.Length; i++)
                {
                    CollectAssemblyReferences(genericArguments[i], assemblyReferences);
                }
            }
            else if (type.IsArray)
            {
                CollectNamespaces(type.GetElementType(), assemblyReferences);
            }
        }

        public override bool EditComponent(ITypeDescriptorContext context, object component, IServiceProvider provider, IWin32Window owner)
        {
            var scriptComponent = (CSharpScript)component;
            if (scriptComponent == null || provider == null) return false;

            var workflowBuilder = (WorkflowBuilder)provider.GetService(typeof(WorkflowBuilder));
            var commandExecutor = (CommandExecutor)provider.GetService(typeof(CommandExecutor));
            var editorService = (IWorkflowEditorService)provider.GetService(typeof(IWorkflowEditorService));
            var selectionModel = (WorkflowSelectionModel)provider.GetService(typeof(WorkflowSelectionModel));
            var scriptEnvironment = (IScriptEnvironment)provider.GetService(typeof(IScriptEnvironment));
            if (workflowBuilder == null || commandExecutor == null || selectionModel == null || scriptEnvironment == null)
            {
                return false;
            }

            var selectedView = selectionModel.SelectedView;
            if (selectedView == null) return false;

            var selectedNode = selectionModel.SelectedNodes.SingleOrDefault();
            if (selectedNode == null) return false;

            string typeName;
            string scriptFile;
            var inputType = default(Type);
            var scriptName = scriptComponent.Category + DefaultScriptName;
            using (var codeProvider = new CSharpCodeProvider())
            {
                var builderNode = (Node<ExpressionBuilder, ExpressionBuilderArgument>)selectedNode.Tag;
                var predecessor = selectedView.Workflow.Predecessors(builderNode).FirstOrDefault();
                if (predecessor != null)
                {
                    var expression = workflowBuilder.Workflow.Build(predecessor.Value);
                    if (expression.Type == typeof(void))
                    {
                        throw new InvalidOperationException(
                            "Script generation failed because the input type could not be determined. " +
                            "Please ensure that the preceding node has a valid output and that all " +
                            "other generated scripts have been successfully compiled.");
                    }

                    inputType = expression.Type;
                }

                var typeReference = CreateTypeReference(inputType ?? typeof(IObservable<int>));
                typeName = codeProvider.GetTypeOutput(typeReference);

                var extensionsDirectory = editorService.EnsureExtensionsDirectory();
                if (!extensionsDirectory.Exists) return false;

                using (var dialog = new SaveFileDialog { InitialDirectory = extensionsDirectory.FullName, FileName = scriptName, Filter = ScriptFilter })
                {
                    if (dialog.ShowDialog() != DialogResult.OK) return false;
                    scriptFile = dialog.FileName;
                    scriptName = Path.GetFileNameWithoutExtension(scriptFile);
                    if (!codeProvider.IsValidIdentifier(scriptName))
                    {
                        throw new InvalidOperationException(
                            "The specified name '" + scriptName + "' is not a valid type identifier. " +
                            "Valid identifiers must start with a letter and must not contain white spaces.");
                    }
                }
            }

            if (scriptEnvironment.AssemblyName != null)
            {
                var existingType = Type.GetType(scriptName + ", " + scriptEnvironment.AssemblyName.FullName);
                if (existingType != null)
                {
                    throw new InvalidOperationException("An extension type with the name " + scriptName + " already exists.");
                }
            }

            var namespaces = new HashSet<string>();
            var assemblyReferences = new HashSet<string>();
            assemblyReferences.Add("Bonsai.Core");
            namespaces.Add("Bonsai");
            namespaces.Add("System");
            namespaces.Add("System.ComponentModel");
            namespaces.Add("System.Collections.Generic");
            namespaces.Add("System.Linq");
            namespaces.Add("System.Reactive.Linq");
            if (inputType != null)
            {
                CollectNamespaces(inputType, namespaces);
                CollectAssemblyReferences(inputType, assemblyReferences);
                assemblyReferences.ExceptWith(IgnoreAssemblyReferences);
            }
            scriptEnvironment.AddAssemblyReferences(assemblyReferences);

            var scriptBuilder = new StringBuilder();
            foreach (var ns in namespaces) scriptBuilder.AppendLine("using " + ns + ";");
            scriptBuilder.AppendLine();
            scriptBuilder.AppendLine("[Combinator]");
            scriptBuilder.AppendLine("[Description(\"\")]");
            scriptBuilder.AppendLine("[WorkflowElementCategory(ElementCategory." + scriptComponent.Category + ")]");
            scriptBuilder.AppendLine("public class " + scriptName);
            scriptBuilder.AppendLine("{");
            scriptBuilder.AppendLine("    public " + typeName + " Process(" + (inputType != null ? typeName + " source)" : ")"));
            scriptBuilder.AppendLine("    {");
            string template;
            switch (scriptComponent.Category)
            {
                case ElementCategory.Source: template = "Observable.Return(0)"; break;
                case ElementCategory.Transform: template = "source.Select(value => value)"; break;
                case ElementCategory.Sink: template = "source.Do(value => Console.WriteLine(value))"; break;
                case ElementCategory.Combinator: template = "source"; break;
                default: throw new InvalidOperationException("The specified element category is not allowed for automatic script generation.");
            }
            scriptBuilder.AppendLine("        return " + template + ";");
            scriptBuilder.AppendLine("    }");
            scriptBuilder.AppendLine("}");

            using (var writer = new StreamWriter(scriptFile))
            {
                writer.Write(scriptBuilder);
            }

            if (moduleBuilder == null)
            {
                var assemblyName = new AssemblyName("@DynamicExtensions");
                var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
                var iconAttributeBuilder = new CustomAttributeBuilder(
                    typeof(WorkflowNamespaceIconAttribute).GetConstructor(new[] { typeof(string) }),
                    new object[] { "Bonsai:ElementIcon.CSharp" });
                assemblyBuilder.SetCustomAttribute(iconAttributeBuilder);
                moduleBuilder = assemblyBuilder.DefineDynamicModule(assemblyName.FullName);

                var emptyExpressionBuilder = moduleBuilder.DefineType("@EmptyExpression", TypeAttributes.Class, typeof(Expression));
                var propertyMethodAttributes = MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.Virtual | MethodAttributes.HideBySig;
                var nodeTypeProperty = emptyExpressionBuilder.DefineProperty("NodeType", PropertyAttributes.None, typeof(ExpressionType), null);
                var nodeTypeGet = emptyExpressionBuilder.DefineMethod("get_NodeType", propertyMethodAttributes, typeof(ExpressionType), Type.EmptyTypes);
                var nodeTypeGetGenerator = nodeTypeGet.GetILGenerator();
                nodeTypeGetGenerator.Emit(OpCodes.Ldc_I4, (int)ExpressionType.Extension);
                nodeTypeGetGenerator.Emit(OpCodes.Ret);
                nodeTypeProperty.SetGetMethod(nodeTypeGet);

                var typeProperty = emptyExpressionBuilder.DefineProperty("Type", PropertyAttributes.None, typeof(Type), null);
                var typePropertyGet = emptyExpressionBuilder.DefineMethod("get_Type", propertyMethodAttributes, typeof(Type), Type.EmptyTypes);
                var typeGetGenerator = typePropertyGet.GetILGenerator();
                var typeGetExceptionConstructor = typeof(InvalidOperationException).GetConstructor(new[] { typeof(string) });
                typeGetGenerator.Emit(OpCodes.Ldstr, Resources.UncompiledScriptExpression_Error);
                typeGetGenerator.Emit(OpCodes.Newobj, typeGetExceptionConstructor);
                typeGetGenerator.Emit(OpCodes.Throw);
                typeProperty.SetGetMethod(typePropertyGet);

                var emptyExpressionType = emptyExpressionBuilder.CreateType();
                emptyExpression = emptyExpressionType.GetConstructor(Type.EmptyTypes);
            }

            var typeBuilder = moduleBuilder.DefineType(
                scriptName,
                TypeAttributes.Public | TypeAttributes.Class,
                inputType == null ? typeof(ZeroArgumentExpressionBuilder) : typeof(SingleArgumentExpressionBuilder));
            var descriptionAttributeBuilder = new CustomAttributeBuilder(
                typeof(DescriptionAttribute).GetConstructor(new[] { typeof(string) }),
                new object[] { "Extensions must be reloaded in order to compile and use the script." });
            var categoryAttributeBuilder = new CustomAttributeBuilder(
                typeof(WorkflowElementCategoryAttribute).GetConstructor(new[] { typeof(ElementCategory) }),
                new object[] { scriptComponent.Category });
            typeBuilder.SetCustomAttribute(descriptionAttributeBuilder);
            typeBuilder.SetCustomAttribute(categoryAttributeBuilder);
            var buildMethod = typeBuilder.DefineMethod("Build",
                MethodAttributes.Public |
                MethodAttributes.ReuseSlot |
                MethodAttributes.Virtual |
                MethodAttributes.HideBySig,
                typeof(Expression),
                new[] { typeof(IEnumerable<Expression>) });
            var generator = buildMethod.GetILGenerator();
            generator.Emit(OpCodes.Newobj, emptyExpression);
            generator.Emit(OpCodes.Ret);

            var type = typeBuilder.CreateType();
            var builder = new CombinatorBuilder();
            builder.Combinator = (object)Activator.CreateInstance(type);
            selectedView.CreateGraphNode(builder, selectedNode, CreateGraphNodeType.Successor, branch: false, validate: false);
            selectedView.DeleteGraphNodes(selectionModel.SelectedNodes);
            commandExecutor.Execute(() => { }, null);

            ScriptEditorLauncher.Launch(owner, scriptEnvironment.ProjectFileName, scriptFile);
            return true;
        }
    }
}
