using System.Globalization;

namespace Bonsai.NuGet.Design
{
    internal static class FormatHelper
    {
        public static string ToLargeSuffix(long count)
        {
            return count switch
            {
                > 999999999 => count.ToString("0,,,.#B", CultureInfo.InvariantCulture),
                > 999999 => count.ToString("0,,.#M", CultureInfo.InvariantCulture),
                > 999 => count.ToString("0,.#K", CultureInfo.InvariantCulture),
                _ => count.ToString(CultureInfo.InvariantCulture)
            };
        }
    }
}
