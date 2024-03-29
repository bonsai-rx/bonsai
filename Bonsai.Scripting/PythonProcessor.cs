﻿using Microsoft.Scripting.Hosting;
using System;

namespace Bonsai.Scripting
{
    class PythonProcessor<TSource, TResult>
    {
        internal PythonProcessor(ScriptScope scope)
        {
            scope.TryGetVariable(PythonHelper.LoadFunction, out Action load);
            scope.TryGetVariable(PythonHelper.UnloadFunction, out Action unload);
            Process = scope.GetVariable<Func<TSource, TResult>>(PythonHelper.ProcessFunction);
            Load = load;
            Unload = unload;
        }

        internal PythonProcessor(ObjectOperations op, object processorClass)
        {
            var processor = (object)op.CreateInstance(processorClass);
            Process = op.GetMember<Func<TSource, TResult>>(processor, PythonHelper.ProcessFunction);
            if (op.ContainsMember(processor, PythonHelper.UnloadFunction))
            {
                Unload = op.GetMember<Action>(processor, PythonHelper.UnloadFunction);
            }
            if (op.ContainsMember(processor, PythonHelper.LoadFunction))
            {
                Load = op.GetMember<Action>(processor, PythonHelper.LoadFunction);
            }
        }

        internal Action Load { get; private set; }

        internal Action Unload { get; private set; }

        internal Func<TSource, TResult> Process { get; private set; }
    }
}
