using System;
using System.ComponentModel.Design;
using System.Drawing.Design;

namespace Bonsai.Design
{
    /// <summary>
    /// Provides an abstract base class for legacy configuration controls.
    /// </summary>
    [Obsolete]
    public abstract class ConfigurationControlBase : ConfigurationDropDown
    {
        /// <inheritdoc/>
        protected override UITypeEditor CreateConfigurationEditor(Type type)
        {
            return CreateCollectionEditor(type);
        }

        /// <summary>
        /// When overridden in a derived class, creates the custom collection editor
        /// for the specified type.
        /// </summary>
        /// <param name="type">
        /// The type of values stored in the configuration collection object.
        /// </param>
        /// <returns>
        /// An instance of the <see cref="CollectionEditor"/> class used to edit
        /// the configuration collection.
        /// </returns>
        protected abstract CollectionEditor CreateCollectionEditor(Type type);
    }

    /// <summary>
    /// Provides an abstract base class for legacy configuration controls.
    /// </summary>
    [Obsolete]
    public abstract class ConfigurationControl : ConfigurationControlBase
    {
        /// <inheritdoc/>
        protected override CollectionEditor CreateCollectionEditor(Type type)
        {
            return CreateConfigurationEditor(type);
        }

        /// <summary>
        /// When overridden in a derived class, creates the custom collection editor
        /// for the specified type.
        /// </summary>
        /// <param name="type">
        /// The type of values stored in the configuration collection object.
        /// </param>
        /// <returns>
        /// An instance of the <see cref="CollectionEditor"/> class used to edit
        /// the configuration collection.
        /// </returns>
        protected abstract new CollectionEditor CreateConfigurationEditor(Type type);
    }
}
