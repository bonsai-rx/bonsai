using Bonsai;
using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;

namespace $rootnamespace$
{
    [Description("")]
    [Combinator(MethodName = nameof(Generate))]
    [WorkflowElementCategory(ElementCategory.Source)]
    public class $safeitemname$
    {
        public IObservable<int> Generate()
        {
            return Observable.Return(0);
        }
    }
}
