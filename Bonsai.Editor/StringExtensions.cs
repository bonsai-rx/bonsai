using System;
using System.Text;

namespace Bonsai.Editor
{
    internal static class StringExtensions
    {
        static bool IsLowerToUpperCase(char a, char b)
        {
            return char.IsLower(a) && char.IsUpper(b);
        }

        static bool IsUpperToLowerCase(char a, char b)
        {
            return char.IsUpper(a) && char.IsLower(b);
        }

        static bool IsWordSeparator(char c)
        {
            return char.IsSeparator(c) || char.IsPunctuation(c);
        }

        static bool IsWordBoundary(string text, int index, char current)
        {
            var previous = text[index - 1];
            return IsLowerToUpperCase(previous, current)
                || IsWordSeparator(previous)
                || index < text.Length - 1 && IsUpperToLowerCase(current, text[index + 1]);
        }

        public static string[] SplitOnWordBoundaries(this string text)
        {
            if (text == null)
            {
                throw new ArgumentNullException(nameof(text));
            }

            var wordCount = 0;
            var words = new string[text.Length];
            var builder = new StringBuilder(text.Length);
            for (int i = 0; i < text.Length; i++)
            {
                var c = text[i];
                if (i > 0 && IsWordBoundary(text, i, c))
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
