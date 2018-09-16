using Bonsai.Expressions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Editor.Scripting
{
    [Editor(typeof(ScriptComponentEditor), typeof(ComponentEditor))]
    abstract class CSharpScript : ExpressionBuilder
    {
        internal const string ActivationInstructions = " Double-click the operator node to create and edit the script file.";

        internal CSharpScript(ElementCategory category)
        {
            Category = category;
        }

        [Browsable(false)]
        public ElementCategory Category { get; private set; }

        public override Expression Build(IEnumerable<Expression> arguments)
        {
            return EmptyExpression.Instance;
        }

        class EmptyExpression : Expression
        {
            internal static readonly EmptyExpression Instance = new EmptyExpression();

            private EmptyExpression()
            {
            }

            public override ExpressionType NodeType
            {
                get { return ExpressionType.Extension; }
            }

            public override Type Type
            {
                get { throw new InvalidOperationException("Unable to evaluate uncompiled script expression. Please ensure all script files have been created and reload extensions."); }
            }
        }
    }

    [WorkflowElementCategory(ElementCategory.Source)]
    [Description("Creates a new source operator backed by a C# script." + ActivationInstructions)]
    class CSharpSource : CSharpScript
    {
        static readonly Range<int> argumentRange = Range.Create(lowerBound: 0, upperBound: 0);

        public CSharpSource()
            : base(ElementCategory.Source)
        {
        }

        public override Range<int> ArgumentRange
        {
            get { return argumentRange; }
        }
    }

    [WorkflowElementCategory(ElementCategory.Combinator)]
    [Description("Creates a new reactive combinator backed by a C# script." + ActivationInstructions)]
    class CSharpCombinator : CSharpScript
    {
        static readonly Range<int> argumentRange = Range.Create(lowerBound: 1, upperBound: 1);

        public CSharpCombinator()
            : base(ElementCategory.Combinator)
        {
        }

        internal CSharpCombinator(ElementCategory category)
            : base(category)
        {
        }

        public override Range<int> ArgumentRange
        {
            get { return argumentRange; }
        }
    }

    [WorkflowElementCategory(ElementCategory.Transform)]
    [Description("Creates a new transform operator backed by a C# script." + ActivationInstructions)]
    class CSharpTransform : CSharpCombinator
    {
        public CSharpTransform()
            : base(ElementCategory.Transform)
        {
        }
    }

    [WorkflowElementCategory(ElementCategory.Condition)]
    [Description("Creates a new condition operator backed by a C# script." + ActivationInstructions)]
    class CSharpCondition : CSharpCombinator
    {
        public CSharpCondition()
            : base(ElementCategory.Condition)
        {
        }
    }

    [WorkflowElementCategory(ElementCategory.Sink)]
    [Description("Creates a new sink operator backed by a C# script." + ActivationInstructions)]
    class CSharpSink : CSharpCombinator
    {
        public CSharpSink()
            : base(ElementCategory.Sink)
        {
        }
    }
}
