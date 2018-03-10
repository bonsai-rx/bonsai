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
    [Description("Creates a new extension operator backed by a C# script file.")]
    class CSharpScript : ExpressionBuilder, INamedElement
    {
        static readonly Range<int> argumentRange = Range.Create(lowerBound: 0, upperBound: 1);

        public CSharpScript()
        {
            Category = ElementCategory.Transform;
        }

        [Category("Design")]
        [Externalizable(false)]
        [Description("The name of the extension operator.")]
        public string Name { get; set; }

        [Category("Design")]
        [Externalizable(false)]
        [Description("An optional description for the extension operator.")]
        [Editor(DesignTypes.MultilineStringEditor, "System.Drawing.Design.UITypeEditor, System.Drawing, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
        public string Description { get; set; }

        [Category("Design")]
        [Externalizable(false)]
        [TypeConverter(typeof(ElementCategoryConverter))]
        [Description("The category of the extension operator to generate.")]
        public ElementCategory Category { get; set; }

        public override Range<int> ArgumentRange
        {
            get { return argumentRange; }
        }

        public override Expression Build(IEnumerable<Expression> arguments)
        {
            return EmptyExpression;
        }

        class ElementCategoryConverter : EnumConverter
        {
            public ElementCategoryConverter()
                : base(typeof(ElementCategory))
            {
            }

            public override TypeConverter.StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
            {
                return new StandardValuesCollection(new[]
                {
                    ElementCategory.Source,
                    ElementCategory.Transform,
                    ElementCategory.Condition,
                    ElementCategory.Combinator,
                    ElementCategory.Sink
                });
            }
        }
    }
}
