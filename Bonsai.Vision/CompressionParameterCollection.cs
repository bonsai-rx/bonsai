using System.Collections.ObjectModel;

namespace Bonsai.Vision
{
    /// <summary>
    /// Represents a collection of compression parameters used to encode individual
    /// images in the <see cref="EncodeImage"/> operator.
    /// </summary>
    public class CompressionParameterCollection : Collection<CompressionParameterAssignment>
    {
        internal int[] GetParameters()
        {
            var result = new int[Count * 2];
            for (int i = 0; i < Count; i++)
            {
                var parameter = Items[i];
                result[i * 2 + 0] = parameter.ParameterType;
                result[i * 2 + 1] = parameter.ParameterValue;
            }
            return result;
        }
    }
}
