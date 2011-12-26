using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Reactive.Linq;
using System.Windows.Forms;

namespace Bonsai.Design
{
    public static class DynamicObservable
    {
        static readonly MethodInfo ObserveOnMethod = typeof(ControlObservable).GetMethod("ObserveOn");
        static readonly MethodInfo SubscribeMethod = typeof(ObservableExtensions).GetMethods().First(m => m.Name == "Subscribe" && m.GetParameters().Length == 2);

        public static Type ObservableType(object observable)
        {
            return observable.GetType()
                .GetInterfaces()
                .Where(type => type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IObservable<>))
                .Select(type => type.GetGenericArguments()[0])
                .First();
        }

        public static object ObserveOn(object observable, Control control)
        {
            var observeOn = ObserveOnMethod.MakeGenericMethod(ObservableType(observable));
            return observeOn.Invoke(null, new[] { observable, control });
        }

        public static object Subscribe(object observable, Delegate onNext)
        {
            var subscribe = SubscribeMethod.MakeGenericMethod(ObservableType(observable));
            return subscribe.Invoke(null, new[] { observable, onNext });
        }
    }
}
