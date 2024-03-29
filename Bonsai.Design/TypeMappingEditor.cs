﻿using Bonsai.Expressions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace Bonsai.Design
{
    /// <summary>
    /// Provides a user interface for selecting method overloads from a workflow operator.
    /// </summary>
    public class TypeMappingEditor : UITypeEditor
    {
        /// <inheritdoc/>
        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.Modal;
        }

        static IEnumerable<MethodInfo> GetProcessMethods(Type combinatorType)
        {
            const BindingFlags bindingAttributes = BindingFlags.Instance | BindingFlags.Public;
            var combinatorAttributes = combinatorType.GetCustomAttributes(typeof(CombinatorAttribute), true);
            if (combinatorAttributes.Length != 1) return Enumerable.Empty<MethodInfo>();
            var methodName = ((CombinatorAttribute)combinatorAttributes.Single()).MethodName;
            return combinatorType.GetMethods(bindingAttributes).Where(m => m.Name == methodName);
        }

        static TypeMapping CreateTypeMapping(Type targetType)
        {
            var mappingType = typeof(TypeMapping<>).MakeGenericType(targetType);
            return (TypeMapping)Activator.CreateInstance(mappingType);
        }

        /// <inheritdoc/>
        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            var editorService = (IWindowsFormsEditorService)provider.GetService(typeof(IWindowsFormsEditorService));
            if (context != null && editorService != null)
            {
                var workflow = (ExpressionBuilderGraph)context.GetService(typeof(ExpressionBuilderGraph));
                if (workflow != null)
                {
                    var builderNode = workflow.FirstOrDefault(node =>
                        ExpressionBuilder.GetWorkflowElement(node.Value) == context.Instance);
                    if (builderNode != null)
                    {
                        var intersection = default(HashSet<Type>);
                        foreach (var successor in builderNode.Successors)
                        {
                            var combinator = ExpressionBuilder.Unwrap(successor.Target.Value) switch
                            {
                                CombinatorBuilder combinatorBuilder => combinatorBuilder.Combinator,
                                ICustomTypeDescriptor typeDescriptor =>
                                    TypeDescriptor.GetDefaultProperty(typeDescriptor.GetType())?.GetValue(typeDescriptor),
                                _ => null
                            };

                            if (combinator == null) continue;
                            var inputTypes = from method in GetProcessMethods(combinator.GetType())
                                             where !method.IsGenericMethod
                                             let parameters = method.GetParameters()
                                             where parameters.Length > successor.Label.Index &&
                                                   parameters[successor.Label.Index].ParameterType.IsGenericType
                                             select parameters[successor.Label.Index].ParameterType.GetGenericArguments()[0];
                            if (intersection == null) intersection = new HashSet<Type>(inputTypes);
                            else intersection.IntersectWith(inputTypes);
                        }

                        var values = new List<TypeMapping> { null };
                        if (intersection != null)
                        {
                            values.AddRange(intersection.Select(CreateTypeMapping));
                        }

                        using (var editorDialog = new TypeMappingEditorDialog(values))
                        {
                            editorDialog.Mapping = (TypeMapping)value;
                            editorDialog.Converter = context.PropertyDescriptor.Converter;
                            if (editorService.ShowDialog(editorDialog) == DialogResult.OK)
                            {
                                return editorDialog.Mapping;
                            }
                        }
                    }
                }
            }

            return base.EditValue(context, provider, value);
        }
    }
}
