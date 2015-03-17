using IronPython.Runtime;
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

        internal static bool TryGetOutputType(ScriptScope scope, string functionName, out Type outputType)
        {
            object returnType;
            var function = scope.GetVariable<PythonFunction>(functionName);
            if (function.func_dict.TryGetValue(PythonHelper.ReturnTypeAttribute, out returnType))
            {
                outputType = (Type)returnType;
                return true;
            }
            else
            {
                Func<Type> getOutputType;
                if (scope.TryGetVariable<Func<Type>>("getOutputType", out getOutputType))
                {
                    outputType = getOutputType();
                    return true;
                }
                else
                {
                    outputType = typeof(object);
                    return false;
                }
            }
        }
    }
}
