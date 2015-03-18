using IronPython.Runtime;
using IronPython.Runtime.Types;
using Microsoft.Scripting.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Scripting
{
    static class PythonHelper
    {
        internal const string ReturnsDecorator = "import clr\ndef returns(type):\n  def decorator(func):\n    func.__returntype__ = clr.GetClrType(type)\n    return func\n  return decorator\n\n";
        internal const string ReturnTypeAttribute = "__returntype__";
        internal const string LoadFunction = "load";
        internal const string UnloadFunction = "unload";
        internal const string ProcessFunction = "process";
        internal const string GenerateFunction = "generate";

        internal static bool TryGetClass(ScriptScope scope, string className, out object pythonClass)
        {
            object variable;
            if (scope.TryGetVariable<object>(className, out variable))
            {
                pythonClass = variable as OldClass;
                if (pythonClass != null)
                {
                    return true;
                }

                pythonClass = variable as PythonType;
                return pythonClass != null;
            }

            pythonClass = null;
            return false;
        }

        internal static Type GetOutputType(ObjectOperations op, object pythonClass, string methodName)
        {
            object returnType;
            var function = (PythonFunction)op.GetMember<Method>(pythonClass, methodName).__func__;
            if (function.func_dict.TryGetValue(PythonHelper.ReturnTypeAttribute, out returnType))
            {
                return (Type)returnType;
            }

            return typeof(object);
        }

        internal static Type GetOutputType(ScriptScope scope, string functionName)
        {
            object returnType;
            var function = scope.GetVariable<PythonFunction>(functionName);
            if (function.func_dict.TryGetValue(PythonHelper.ReturnTypeAttribute, out returnType))
            {
                return (Type)returnType;
            }
            else
            {
                Func<Type> getOutputType;
                if (scope.TryGetVariable<Func<Type>>("getOutputType", out getOutputType))
                {
                    return getOutputType();
                }
                else
                {
                    return typeof(object);
                }
            }
        }
    }
}
