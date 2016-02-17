using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Expressions
{
    class BehaviorSubjectTypeDescriptionProvider : TypeDescriptionProvider
    {
        static readonly TypeDescriptionProvider parentProvider = TypeDescriptor.GetProvider(typeof(BinaryOperatorBuilder));

        public BehaviorSubjectTypeDescriptionProvider()
            : base(parentProvider)
        {
        }

        public override ICustomTypeDescriptor GetExtendedTypeDescriptor(object instance)
        {
            return new BehaviorSubjectTypeDescriptor(instance);
        }

        class BehaviorSubjectTypeDescriptor : CustomTypeDescriptor
        {
            BehaviorSubjectBuilder builder;
            static readonly Attribute[] valueAttributes = new[]
            {
                new DescriptionAttribute("The initial value sent to observers when no other value has been received by the subject yet.")
            };

            public BehaviorSubjectTypeDescriptor(object instance)
            {
                if (instance == null)
                {
                    throw new ArgumentNullException("instance");
                }

                builder = (BehaviorSubjectBuilder)instance;
            }

            public override PropertyDescriptorCollection GetProperties()
            {
                return GetProperties(valueAttributes);
            }

            public override PropertyDescriptorCollection GetProperties(Attribute[] attributes)
            {
                var value = builder.Value;
                if (value != null)
                {
                    var propertyDescriptor = new WorkflowPropertyDescriptor("Value", valueAttributes, value);
                    return new PropertyDescriptorCollection(new[] { propertyDescriptor });
                }
                else return PropertyDescriptorCollection.Empty;
            }
        }
    }
}
