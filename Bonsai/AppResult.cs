using System;

namespace Bonsai
{
    static class AppResult
    {
        public static TResult GetResult<TResult>()
        {
            return ResultHolder<TResult>.ResultValue;
        }

        public static void SetResult<TResult>(TResult result)
        {
            ResultHolder<TResult>.ResultValue = result;
        }

        class ResultHolder<TResult>
        {
            public static TResult ResultValue;
        }
    }
}
