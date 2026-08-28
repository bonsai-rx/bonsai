using System;
using System.CodeDom;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using Bonsai.Expressions;
using Microsoft.CSharp;

namespace Bonsai.Design
{
    internal class CallBuilderMethodNameEditor : UITypeEditor
    {
        public override bool IsDropDownResizable => true;

        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.DropDown;
        }

        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            var editorService = (IWindowsFormsEditorService)provider.GetService(typeof(IWindowsFormsEditorService));
            if (editorService is not null && context is not null && context.Instance is CallBuilder callBuilder)
            {
                var workflow = (ExpressionBuilderGraph)context.GetService(typeof(ExpressionBuilderGraph));
                var source = (from node in workflow
                              from successor in node.Successors
                              where ExpressionBuilder.Unwrap(successor.Target.Value) == callBuilder
                              select node.Value).FirstOrDefault();
                if (source is not null)
                {
                    var workflowBuilder = (WorkflowBuilder)context.GetService(typeof(WorkflowBuilder));
                    var sourceExpression = workflowBuilder.Workflow.Build(source);
                    var instanceType = sourceExpression.Type.GetGenericArguments()[0];
                    var instanceExpression = ExpressionHelper.MemberAccess(
                        Expression.Parameter(instanceType),
                        callBuilder.InstanceSelector);
                    var methods = CallBuilder.GetInstanceMethods(instanceExpression.Type).ToList();

                    using var methodPicker = new MethodNamePicker(methods, editorService);
                    editorService.DropDownControl(methodPicker);
                    return methodPicker.SelectedIndex >= 0
                        ? methods[methodPicker.SelectedIndex].Name
                        : value;
                }
            }

            return base.EditValue(context, provider, value);
        }

        class MethodNamePicker : ListBox
        {
            const float DefaultDpi = 96f;
            const int RightMargin = 10;
            const int MaxInitialItems = 10;

            public MethodNamePicker(IEnumerable<MethodInfo> methods, IWindowsFormsEditorService editorService)
            {
                EditorService = editorService ?? throw new ArgumentNullException(nameof(editorService));
                BorderStyle = BorderStyle.None;
                Dock = DockStyle.Fill;
                FormattingEnabled = true;
                IntegralHeight = false;

                var maxBounds = SizeF.Empty;
                using var graphics = Graphics.FromHwnd(IntPtr.Zero);
                using var codeProvider = new CSharpCodeProvider();
                foreach (var method in methods)
                {
                    var displayName = GetMethodOutput(codeProvider, method);
                    var itemSize = graphics.MeasureString(displayName, Font);
                    maxBounds.Width = Math.Max(maxBounds.Width, itemSize.Width);
                    maxBounds.Height = Math.Max(maxBounds.Height, itemSize.Height);
                    Items.Add(displayName);
                }

                var drawScale = graphics.DpiY / DefaultDpi;
                Width = (int)Math.Ceiling(maxBounds.Width + RightMargin * drawScale);
                Height = (int)Math.Ceiling(ItemHeight * Math.Min(Items.Count, MaxInitialItems) * drawScale);
            }

            IWindowsFormsEditorService EditorService { get; }

            protected override void OnSelectedValueChanged(EventArgs e)
            {
                EditorService.CloseDropDown();
                base.OnSelectedValueChanged(e);
            }

            static string GetMethodOutput(CSharpCodeProvider codeProvider, MethodInfo method)
            {
                const string ListSeparator = ", ";
                var returnType = codeProvider.GetTypeOutput(GetTypeReference(method.ReturnType));
                var parameterTypes = Array.ConvertAll(
                    method.GetParameters(),
                    parameter => codeProvider.GetTypeOutput(GetTypeReference(parameter.ParameterType)));
                var typeArguments = method.ContainsGenericParameters ? $"<{string.Join(ListSeparator, Array.ConvertAll(
                    method.GetGenericArguments(),
                    parameter => codeProvider.GetTypeOutput(GetTypeReference(parameter))))}>"
                    : string.Empty;
                return $"{returnType} {method.Name}{typeArguments}({string.Join(ListSeparator, parameterTypes)})";
            }

            static CodeTypeReference GetTypeReference(Type type)
            {
                var baseType = type.IsArray || type.IsPointer || type.IsByRef ? type.GetElementType() : type;
                if (baseType.IsPrimitive || baseType == typeof(string) || baseType == typeof(object))
                {
                    return new CodeTypeReference(type);
                }

                var reference = new CodeTypeReference(type);
                if (type.IsArray) reference.ArrayRank = type.GetArrayRank();
                if (type.IsGenericType)
                {
                    var typeParameters = type.GetGenericArguments();
                    reference.TypeArguments.AddRange(Array.ConvertAll(typeParameters, GetTypeReference));
                }
                return reference;
            }
        }
    }
}
