using System;
using System.ComponentModel;
using Getter = System.Func<object, object>;
using Setter = System.Action<object, object>;

namespace Bonsai.Design
{
    /// <summary>
    /// Provides a virtual property for a class.
    /// </summary>
    public class DynamicPropertyDescriptor : PropertyDescriptor
    {
        readonly Type propertyType;
        readonly Getter getValue;
        readonly Setter setValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicPropertyDescriptor"/> class
        /// using the specified name, type, and dynamic methods for getting and setting
        /// property values.
        /// </summary>
        /// <param name="name">The name of the dynamic property.</param>
        /// <param name="type">The type of the dynamic property.</param>
        /// <param name="getter">A method used to get the current value of the dynamic property.</param>
        /// <param name="setter">
        /// A method used to set the value of the dynamic property to a different value.
        /// </param>
        /// <param name="attributes">
        /// An optional array of <see cref="Attribute"/> objects that contains the property attributes.
        /// </param>
        public DynamicPropertyDescriptor(string name, Type type, Getter getter, Setter setter, params Attribute[] attributes)
            : base(name, attributes)
        {
            propertyType = type;
            getValue = getter;
            setValue = setter;
        }

        /// <summary>
        /// Returns whether resetting an object changes its value. Dynamic
        /// properties do not support resetting, so resetting an object
        /// never changes its value.
        /// </summary>
        /// <inheritdoc/>
        public override bool CanResetValue(object component)
        {
            return false;
        }

        /// <summary>
        /// Gets the type of the component this property is bound to.
        /// </summary>
        public override Type ComponentType
        {
            get { return null; }
        }

        /// <summary>
        /// Gets the current value of the property on a component.
        /// </summary>
        /// <inheritdoc/>
        public override object GetValue(object component)
        {
            return getValue(component);
        }

        /// <summary>
        /// Gets a value indicating whether this property is read-only.
        /// </summary>
        public override bool IsReadOnly
        {
            get { return setValue == null; }
        }

        /// <summary>
        /// Gets the type of the dynamic property.
        /// </summary>
        public override Type PropertyType
        {
            get { return propertyType; }
        }

        /// <summary>
        /// Resets the value for this property of the component to the default value.
        /// Dynamic properties do not support resetting their values.
        /// </summary>
        /// <inheritdoc/>
        public override void ResetValue(object component)
        {
        }

        /// <summary>
        /// Sets the value of the dynamic property to a different value.
        /// </summary>
        /// <inheritdoc/>
        public override void SetValue(object component, object value)
        {
            if (setValue == null)
            {
                throw new InvalidOperationException("Tried to set a value on a read-only dynamic property.");
            }

            setValue(component, value);
        }

        /// <summary>
        /// Determines a value indicating whether the value of this property needs
        /// to be persisted. Dynamic property values are transient, so they always
        /// need to be persisted.
        /// </summary>
        /// <inheritdoc/>
        public override bool ShouldSerializeValue(object component)
        {
            return true;
        }
    }
}
