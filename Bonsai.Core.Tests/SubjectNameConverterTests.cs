using System;
using System.ComponentModel;
using System.Linq;
using Bonsai.Expressions;
using Bonsai.Reactive;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Bonsai.Core.Tests
{
    [TestClass]
    public class SubjectNameConverterTests
    {
        const string BaseSubjectName = "Base";
        const string DerivedSubjectName = "Derived";

        class WorkflowTypeDescriptorContext : ITypeDescriptorContext
        {
            readonly WorkflowBuilder workflowBuilder;

            public WorkflowTypeDescriptorContext(WorkflowBuilder workflowBuilder, object instance, PropertyDescriptor descriptor)
            {
                this.workflowBuilder = workflowBuilder;
                Instance = instance;
                PropertyDescriptor = descriptor;
            }

            public IContainer Container => null;

            public object Instance { get; }

            public PropertyDescriptor PropertyDescriptor { get; }

            public void OnComponentChanged() { }

            public bool OnComponentChanging() => true;

            public object GetService(Type serviceType)
            {
                if (serviceType == typeof(WorkflowBuilder)) return workflowBuilder;
                if (serviceType == typeof(ExpressionBuilderGraph)) return workflowBuilder.Workflow;
                return null;
            }
        }

        static TestWorkflow DeclareSubjects()
        {
            return new TestWorkflow()
                .AppendValue<PolymorphicType>(new PolyType())
                .AppendSubject<BehaviorSubject>(BaseSubjectName)
                .ResetCursor()
                .AppendValue(new PolyType())
                .AppendSubject<BehaviorSubject>(DerivedSubjectName)
                .ResetCursor();
        }

        static string[] GetSubjectNames(TestWorkflow workflow, ExpressionBuilder builder)
        {
            var workflowBuilder = new WorkflowBuilder(workflow.ToInspectableGraph());
            workflowBuilder.Workflow.Build();
            var descriptor = TypeDescriptor.GetProperties(builder)[nameof(MulticastSubject.Name)];
            var context = new WorkflowTypeDescriptorContext(workflowBuilder, builder, descriptor);
            return descriptor.Converter.GetStandardValues(context).Cast<string>().ToArray();
        }

        [TestMethod]
        public void GetStandardValues_MulticastDerivedSource_IncludesBaseTypeSubject()
        {
            var multicast = new MulticastSubject();
            var workflow = DeclareSubjects()
                .AppendValue(new PolyType())
                .Append(multicast);
            var names = GetSubjectNames(workflow, multicast);
            CollectionAssert.AreEquivalent(new[] { BaseSubjectName, DerivedSubjectName }, names);
        }

        [TestMethod]
        public void GetStandardValues_MulticastBaseSource_ExcludesDerivedTypeSubject()
        {
            var multicast = new MulticastSubject();
            var workflow = DeclareSubjects()
                .AppendValue<PolymorphicType>(new PolyType())
                .Append(multicast);
            var names = GetSubjectNames(workflow, multicast);
            CollectionAssert.AreEquivalent(new[] { BaseSubjectName }, names);
        }
    }
}
