using System;
using System.Collections.Generic;
using System.IO;
using System.Reactive.Disposables;
using Newtonsoft.Json;

namespace Bonsai
{
    static class AppResult
    {
        static readonly JsonSerializer Serializer = JsonSerializer.CreateDefault();
        static Dictionary<string, string> Values;

        public static IDisposable OpenWrite(Stream stream)
        {
            Values = new();
            if (stream == null)
            {
                return Disposable.Empty;
            }

            var writer = new JsonTextWriter(new StreamWriter(stream));
            return Disposable.Create(() =>
            {
                try { Serializer.Serialize(writer, Values); }
                finally { writer.Close(); }
            });
        }

        public static IDisposable OpenRead(Stream stream)
        {
            using var reader = new JsonTextReader(new StreamReader(stream));
            Values = Serializer.Deserialize<Dictionary<string, string>>(reader);
            return Disposable.Empty;
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
    }
}
