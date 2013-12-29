using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Bonsai
{
    /// <summary>
    /// Provides methods for converting between instances of the <see cref="Array"/> class
    /// and <see cref="String"/>.
    /// </summary>
    public static class ArrayConvert
    {
        const string RowSeparator = ";";
        const string ColumnSeparator = ",";

        /// <summary>
        /// Converts the <see cref="Array"/> to a <see cref="String"/>.
        /// </summary>
        /// <param name="array">The <see cref="Array"/> to be converted.</param>
        /// <returns>
        /// A string representation of the <see cref="Array"/> using the system's
        /// current culture.
        /// </returns>
        public static string ToString(Array array)
        {
            return ToString(array, Thread.CurrentThread.CurrentCulture);
        }

        /// <summary>
        /// Converts the <see cref="Array"/> to a <see cref="String"/> using the specified
        /// culture-specific format information.
        /// </summary>
        /// <param name="array">The <see cref="Array"/> to be converted.</param>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        /// <returns>
        /// A string representation of the <see cref="Array"/> using the specified
        /// culture-specific format information.
        /// </returns>
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

        /// <summary>
        /// Converts the <see cref="String"/> to an <see cref="Array"/> equivalent with
        /// the specified <paramref name="rank"/> and <paramref name="elementType"/>.
        /// </summary>
        /// <param name="value">The string to convert.</param>
        /// <param name="rank">The rank of the result array; can be either one- or two-dimensional.</param>
        /// <param name="elementType">The type of the elements in the array.</param>
        /// <returns>An <see cref="Array"/> equivalent of the string.</returns>
        public static Array ToArray(string value, int rank, Type elementType)
        {
            return ToArray(value, rank, elementType, Thread.CurrentThread.CurrentCulture);
        }

        /// <summary>
        /// Converts the <see cref="String"/> to an <see cref="Array"/> equivalent with
        /// the specified <paramref name="rank"/> and <paramref name="elementType"/> using
        /// a <paramref name="provider"/> of culture-specific formatting information.
        /// </summary>
        /// <param name="value">The string to convert.</param>
        /// <param name="rank">The rank of the result array; can be either one- or two-dimensional.</param>
        /// <param name="elementType">The type of the elements in the array.</param>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        /// <returns>An <see cref="Array"/> equivalent of the string.</returns>
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
