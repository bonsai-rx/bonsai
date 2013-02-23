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

        public static string ToString(Array array)
        {
            return ToString(array, Thread.CurrentThread.CurrentCulture);
        }

        public static string ToString(Array array, IFormatProvider provider)
        {
            if (array != null)
            {
                if (array.Rank > 2)
                {
                    throw new ArgumentException("Array cannot have rank greater than two.", "array");
                }

                var text = string.Empty;
                if (array.Rank == 1)
                {
                    var cols = array.GetLength(0);
                    for (int j = 0; j < cols; j++)
                    {
                        text += Convert.ToString(array.GetValue(j), provider);
                        if (j < cols - 1) text += ColumnSeparator;
                    }
                }
                else if (array.Rank == 2)
                {
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
                }

                return text;
            }

            return null;
        }

        public static Array ToArray(string value, int rank, Type elementType)
        {
            return ToArray(value, rank, elementType, Thread.CurrentThread.CurrentCulture);
        }

        public static Array ToArray(string value, int rank, Type elementType, IFormatProvider provider)
        {
            if (rank > 2)
            {
                throw new ArgumentException("Rank cannot be greater than two.", "rank");
            }

            if (value != null)
            {
                value = value.Trim();
                if (rank == 1)
                {
                    var columnValues = value.Split(new[] { ColumnSeparator }, StringSplitOptions.RemoveEmptyEntries);
                    var instance = Array.CreateInstance(elementType, columnValues.Length);
                    for (int j = 0; j < columnValues.Length; j++)
                    {
                        var element = Convert.ChangeType(columnValues[j], elementType, provider);
                        instance.SetValue(element, j);
                    }

                    return instance;
                }

                if (rank == 2)
                {
                    int? numColumns = null;
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
            }

            return null;
        }
    }
}
