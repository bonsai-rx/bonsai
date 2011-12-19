using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using System.Xml.Serialization;

namespace Bonsai
{
    public class Workflow : Source
    {
        IDisposable processingChain;
        readonly OutputObservable<Exception> error;
        readonly WorkflowElementCollection components;

        public Workflow()
        {
            error = new OutputObservable<Exception>();
            components = new WorkflowElementCollection();
        }

        [XmlIgnore]
        public bool Running { get; private set; }

        public event EventHandler RunningChanged;

        public IObservable<Exception> Error
        {
            get { return error; }
        }

        public WorkflowElementCollection Components
        {
            get { return components; }
        }

        protected virtual void OnRunningChanged(EventArgs e)
        {
            var handler = RunningChanged;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        public override void Start()
        {
            if (Running) return;

            foreach (var component in components)
            {
                var source = component as Source;
                if (source != null) source.Start();
            }

            Running = true;
            OnRunningChanged(EventArgs.Empty);
        }

        public override void Stop()
        {
            if (!Running) return;

            foreach (var component in components)
            {
                var source = component as Source;
                if (source != null) source.Stop();
            }

            Running = false;
            OnRunningChanged(EventArgs.Empty);
        }

        public override void Load(WorkflowContext context)
        {
            Expression output = null;
            ParameterExpression input = null;
            object observableSource = null;
            Type outputType = null;
            foreach (var component in components)
            {
                // Load the workflow element
                component.Load(context);

                // The first workflow element must be an observable source
                if (output == null)
                {
                    var outputProperty = component.GetType().GetProperty("Output");
                    outputType = outputProperty.PropertyType.GetGenericArguments()[0];
                    observableSource = outputProperty.GetValue(component, null);
                    output = input = Expression.Parameter(outputType);
                    continue;
                }

                // Add another processing step to the workflow expression
                var instance = Expression.Constant(component);
                var method = component.GetType().GetMethod("Process");
                output = Expression.Call(instance, method, output);
            }

            // Route any raised exceptions through the observable error stream
            var errorInstance = Expression.Constant(error);
            var errorMethod = error.GetType().GetMethod("OnNext");
            var exception = Expression.Parameter(typeof(Exception));
            output = Expression.TryCatch(output, Expression.Catch(exception, Expression.Call(errorInstance, errorMethod, exception)));

            // Subscribe the compiled processing chain to the observable source
            var observer = Expression.Lambda(output, input).Compile();
            var subscribeMethod = typeof(ObservableExtensions).GetMethods().First(m => m.Name == "Subscribe" && m.GetParameters().Length == 2);
            subscribeMethod = subscribeMethod.MakeGenericMethod(new[] { outputType });
            processingChain = (IDisposable)subscribeMethod.Invoke(null, new[] { observableSource, observer });

            // Add the workflow as a context service
            context.AddService(typeof(Workflow), this);
        }

        public override void Unload(WorkflowContext context)
        {
            // Remove the workflow as a context service
            context.RemoveService(typeof(Workflow));

            // Unsubscribe the processing chain from the observable source
            processingChain.Dispose();

            // Unload the workflow elements in reverse order
            foreach (var component in components.Reverse())
            {
                component.Unload(context);
            }
        }
    }
}
