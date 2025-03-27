using System.Collections.Generic;

namespace Bonsai.Shaders
{
    /// <summary>
    /// Represents a read-only array of texture objects that can be accessed by index.
    /// </summary>
    public interface ITextureArray : IEnumerable<int>
    {
        /// <summary>
        /// Gets the total number of texture objects in the array.
        /// </summary>
        int Length { get; }

        /// <summary>
        /// Gets the texture object at the specified index.
        /// </summary>
        /// <param name="index">
        /// The zero-based index of the texture object to get.
        /// </param>
        /// <returns>
        /// The handle to the texture object at the specified index.
        /// </returns>
        int this[int index] { get; }
    }
}
