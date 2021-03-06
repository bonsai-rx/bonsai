﻿using System;
using System.ComponentModel;

namespace Bonsai.Expressions
{
    class BinaryOperatorTypeDescriptionProvider : TypeDescriptionProvider
    {
        static readonly TypeDescriptionProvider parentProvider = TypeDescriptor.GetProvider(typeof(BinaryOperatorBuilder));

        public BinaryOperatorTypeDescriptionProvider()
            : base(parentProvider)
        {
        }

        public override ICustomTypeDescriptor GetExtendedTypeDescriptor(object instance)
        {
            return new BinaryOperatorTypeDescriptor(instance);
        }

        class BinaryOperatorTypeDescriptor : CustomTypeDescriptor
        {
            readonly BinaryOperatorBuilder builder;
            static readonly Attribute[] valueAttributes = new[]
            {
                new DescriptionAttribute("The value of the right hand operand in the binary operator.")
            };

            public BinaryOperatorTypeDescriptor(object instance)
            {
                if (instance == null)
                {
                    throw new ArgumentNullException(nameof(instance));
                }

                builder = (BinaryOperatorBuilder)instance;
            }

            public override PropertyDescriptorCollection GetProperties()
            {
                return GetProperties(valueAttributes);
            }

            public override PropertyDescriptorCollection GetProperties(Attribute[] attributes)
            {
                var operand = builder.Operand;
                if (operand != null)
                {
                    var propertyDescriptor = new WorkflowPropertyDescriptor("Value", valueAttributes, operand);
                    return new PropertyDescriptorCollection(new[] { propertyDescriptor });
                }
                else return PropertyDescriptorCollection.Empty;
            }
        }
    }
}
