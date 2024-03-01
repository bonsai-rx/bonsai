using System.Collections.Generic;
using NuGet.Protocol.Core.Types;

namespace Bonsai.NuGet
{
    static class QueryHelper
    {
        public static SearchFilter CreateSearchFilter(bool includePrerelease, IEnumerable<string> packageTypes)
        {
            var searchFilterType = includePrerelease ? SearchFilterType.IsAbsoluteLatestVersion : SearchFilterType.IsLatestVersion;
            return new SearchFilter(includePrerelease, searchFilterType)
            {
                PackageTypes = packageTypes
            };
        }
    }
}
