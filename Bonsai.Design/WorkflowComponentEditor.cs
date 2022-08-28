using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace Bonsai.Design
{
    /// <summary>
    /// Provides the abstract base class for a custom workflow component editor.
    /// </summary>
    public abstract class WorkflowComponentEditor : ComponentEditor
    {
        /// <summary>
        /// Edits the component and returns a value indicating whether the component was
        /// modified using the specified service provider and a parent window handle.
        /// </summary>
        /// <param name="component">The component to be edited.</param>
        /// <param name="provider">
        /// A service provider that this editor can use to obtain services.
        /// </param>
        /// <param name="owner">The window handle which contains any editor dialogs.</param>
        /// <returns>
        /// <see langword="true"/> if the component was modified; otherwise, <see langword="false"/>.
        /// </returns>
        public bool EditComponent(object component, IServiceProvider provider, IWin32Window owner)
        {
            return EditComponent(null, component, provider, owner);
        }

        /// <inheritdoc/>
        public override bool EditComponent(ITypeDescriptorContext context, object component)
        {
            return EditComponent(context, component, null, null);
        }

        /// <summary>
        /// Edits the component and returns a value indicating whether the component was
        /// modified using the specified context and a parent window handle.
        /// </summary>
        /// <param name="context">
        /// An optional context object that can be used to obtain further information about the edit.
        /// </param>
        /// <param name="component">The component to be edited.</param>
        /// <param name="provider">
        /// A service provider that this editor can use to obtain services.
        /// </param>
        /// <param name="owner">The window handle which contains any editor dialogs.</param>
        /// <returns>
        /// <see langword="true"/> if the component was modified; otherwise, <see langword="false"/>.
        /// </returns>
        public abstract bool EditComponent(ITypeDescriptorContext context, object component, IServiceProvider provider, IWin32Window owner);
    }
}
