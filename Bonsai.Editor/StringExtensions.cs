using System;
using System.Text;

namespace Bonsai.Editor
{
    internal static class StringExtensions
    {
        public static string[] SplitOnWordBoundaries(this string text)
        {
            var wordCount = 0;
            var words = new string[text.Length];
            var builder = new StringBuilder(text.Length);
            foreach (var c in text)
            {
                if (builder.Length > 0 && (Char.IsUpper(c) || Char.IsWhiteSpace(c)))
                {
                    words[wordCount++] = builder.ToString();
                    builder.Clear();
                }

                builder.Append(c);
            }

            if (builder.Length > 0) words[wordCount++] = builder.ToString();
            Array.Resize(ref words, wordCount);
            return words;
        }
    }
}
