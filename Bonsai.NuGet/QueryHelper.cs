using NuGet.Protocol.Core.Types;

namespace Bonsai.NuGet
{
    public static class QueryHelper
    {
        public static SearchFilter CreateSearchFilter(bool includePrerelease, string packageType)
        {
            var searchFilterType = includePrerelease ? SearchFilterType.IsAbsoluteLatestVersion : SearchFilterType.IsLatestVersion;
            var searchFilter = new SearchFilter(includePrerelease, searchFilterType);
            if (!string.IsNullOrEmpty(packageType))
            {
                searchFilter.PackageTypes = new[] { packageType };
            }

            return searchFilter;
        }
    }
}
