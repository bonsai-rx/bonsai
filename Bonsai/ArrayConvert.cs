using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Bonsai
{
    public static class ArrayConvert
    {
        public const string RowSeparator = ";";
        public const string ColumnSeparator = ",";

        public static string ToString(Array value)
        {
            return ToString(value, Thread.CurrentThread.CurrentCulture);
        }

        public static string ToString(Array value, IFormatProvider provider)
        {
            if (value != null)
            {
                var text = string.Empty;
                var array = (Array)value;
                var rows = array.GetLength(0);
                var cols = array.GetLength(1);
                for (int i = 0; i < rows; i++)
                {
                    for (int j = 0; j < cols; j++)
                    {
                        text += Convert.ToString(array.GetValue(i, j), provider);
                        if (j < cols - 1) text += ColumnSeparator;
                    }

                    if (i < rows - 1) text += RowSeparator;
                }

                return text;
            }

            return null;
        }

        public static TElement[,] ToArray<TElement>(string value)
        {
            return (TElement[,])ToArray(value, typeof(TElement));
        }

        public static Array ToArray(string value, Type elementType)
        {
            return ToArray(value, elementType, Thread.CurrentThread.CurrentCulture);
        }

        public static Array ToArray(string value, Type elementType, IFormatProvider provider)
        {
            if (value != null)
            {
                int? numColumns = null;
                value = value.Trim();
                var rows = value.Split(new[] { RowSeparator }, StringSplitOptions.RemoveEmptyEntries);
                var arrayValues = Array.ConvertAll(rows, row =>
                {
                    var columns = row.Split(new[] { ColumnSeparator }, StringSplitOptions.RemoveEmptyEntries);
                    if (numColumns != null && numColumns != columns.Length)
                    {
                        throw new ArgumentException("Matrix specification must be rectangular.", "value");
                    }

                    numColumns = columns.Length;
                    return columns;
                });

                var instance = Array.CreateInstance(elementType, arrayValues.Length, numColumns.GetValueOrDefault());
                for (int i = 0; i < arrayValues.Length; i++)
                {
                    var columnValues = arrayValues[i];
                    for (int j = 0; j < columnValues.Length; j++)
                    {
                        var element = Convert.ChangeType(arrayValues[i][j], elementType, provider);
                        instance.SetValue(element, i, j);
                    }
                }

                return instance;
            }

            return null;
        }
    }
}
