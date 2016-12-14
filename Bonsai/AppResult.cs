using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai
{
    static class AppResult
    {
        public static TResult GetResult<TResult>(AppDomain domain)
        {
            var resultHolder = (ResultHolder<TResult>)domain.CreateInstanceAndUnwrap(
                typeof(ResultHolder<TResult>).Assembly.FullName,
                typeof(ResultHolder<TResult>).FullName);
            return resultHolder.Result;
        }

        public static void SetResult<TResult>(TResult result)
        {
            ResultHolder<TResult>.ResultValue = result;
        }

        class ResultHolder<TResult> : MarshalByRefObject
        {
            public static TResult ResultValue;

            public ResultHolder()
            {
            }

            public TResult Result
            {
                get { return ResultValue; }
            }
        }
    }
}
