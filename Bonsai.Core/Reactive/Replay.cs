﻿using System;
using System.ComponentModel;
using System.Reactive.Linq;
using System.Xml;
using System.Xml.Serialization;
using Bonsai.Expressions;

namespace Bonsai.Reactive
{
    /// <summary>
    /// Represents an expression builder that handles sharing of an observable sequence
    /// across the encapsulated workflow by eagerly replaying notifications.
    /// </summary>
    [XmlType(Namespace = Constants.ReactiveXmlNamespace)]
    [TypeDescriptionProvider(typeof(ReplayTypeDescriptionProvider))]
    [Description("Shares an observable sequence across the encapsulated workflow by eagerly replaying notifications.")]
    public class Replay : MulticastBuilder
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Replay"/> class.
        /// </summary>
        public Replay()
            : this(new ExpressionBuilderGraph())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Replay"/> class
        /// with the specified expression builder workflow.
        /// </summary>
        /// <param name="workflow">
        /// The expression builder workflow instance that will be used by this builder
        /// to generate the output expression tree.
        /// </param>
        public Replay(ExpressionBuilderGraph workflow)
            : base(workflow)
        {
        }

        /// <summary>
        /// Gets or sets the maximum element count of the replay buffer.
        /// </summary>
        [Category("Replay")]
        [Externalizable(false)]
        [Description("The maximum element count of the replay buffer.")]
        public int? BufferSize { get; set; }

        /// <summary>
        /// Gets or sets the maximum time length of the replay buffer.
        /// </summary>
        [XmlIgnore]
        [Category("Replay")]
        [Externalizable(false)]
        [Description("The maximum time length of the replay buffer.")]
        public TimeSpan? Window { get; set; }

        /// <summary>
        /// Gets or sets the XML serializable representation of the replay window interval.
        /// </summary>
        [Browsable(false)]
        [XmlElement("Window")]
        public string WindowXml
        {
            get
            {
                var window = Window;
                if (window.HasValue) return XmlConvert.ToString(window.Value);
                else return null;
            }
            set
            {
                if (!string.IsNullOrEmpty(value)) Window = XmlConvert.ToTimeSpan(value);
                else Window = null;
            }
        }

        internal override IObservable<TResult> Multicast<TSource, TResult>(IObservable<TSource> source, Func<IObservable<TSource>, IObservable<TResult>> selector)
        {
            var bufferSize = BufferSize;
            var window = Window;
            if (bufferSize.HasValue)
            {
                if (window.HasValue) return source.Replay(selector, bufferSize.Value, window.Value);
                else return source.Replay(selector, bufferSize.Value);
            }
            else if (window.HasValue)
            {
                return source.Replay(selector, window.Value);
            }
            else return source.Replay(selector);
        }

        class ReplayTypeDescriptionProvider : TypeDescriptionProvider
        {
            static readonly TypeDescriptionProvider parentProvider = TypeDescriptor.GetProvider(typeof(Replay));

            public ReplayTypeDescriptionProvider()
                : base(parentProvider)
            {
            }

            public override ICustomTypeDescriptor GetExtendedTypeDescriptor(object instance)
            {
                return new WorkflowTypeDescriptor(instance);
            }
        }
    }
}
