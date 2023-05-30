using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Threading;
using Newtonsoft.Json;

namespace Bonsai
{
    static class AppResult
    {
        static readonly JsonSerializer Serializer = JsonSerializer.CreateDefault();
        static Dictionary<string, string> Values;

        public static IDisposable OpenWrite(NamedPipeClientStream stream)
        {
            Values = new();
            if (stream == null)
            {
                return EmptyDisposable.Instance;
            }

            stream.Connect();
            var writer = new StreamWriter(stream);
            return new AnonymousDisposable(() =>
            {
                try
                {
                    Serializer.Serialize(writer, Values);
                    writer.Flush();
                    try { stream.WaitForPipeDrain(); }
                    catch (NotSupportedException) { }
                }
                finally { writer.Close(); }
            });
        }

        public static IDisposable OpenRead(Stream stream)
        {
            using var reader = new JsonTextReader(new StreamReader(stream));
            Values = Serializer.Deserialize<Dictionary<string, string>>(reader);
            return EmptyDisposable.Instance;
        }

        public static TResult GetResult<TResult>()
        {
            if (Values == null)
            {
                throw new InvalidOperationException("No output stream has been opened for reading.");
            }

            if (Values.TryGetValue(typeof(TResult).FullName, out string value))
            {
                if (typeof(TResult).IsEnum)
                {
                    return (TResult)Enum.Parse(typeof(TResult), value);
                }

                return (TResult)(object)value;
            }

            return default;
        }

        public static void SetResult<TResult>(TResult result)
        {
            if (Values == null)
            {
                throw new InvalidOperationException("No output stream has been opened for writing.");
            }

            Values[typeof(TResult).FullName] = result.ToString();
        }

        class AnonymousDisposable : IDisposable
        {
            private Action disposeAction;

            public AnonymousDisposable(Action dispose)
            {
                disposeAction = dispose;
            }

            public void Dispose()
            {
                Interlocked.Exchange(ref disposeAction, null)?.Invoke();
            }
        }

        class EmptyDisposable : IDisposable
        {
            public static readonly EmptyDisposable Instance = new();

            private EmptyDisposable()
            {
            }

            public void Dispose()
            {
            }
        }
    }
}
