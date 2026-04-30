using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;

namespace Suity.Editor.Helpers;

/// <summary>
/// Provides utility methods for working with HTTP headers.
/// </summary>
public static class HttpHelper
{
    /// <summary>
    /// Retrieves the first value associated with the specified header name.
    /// </summary>
    /// <param name="header">The HTTP headers collection to search.</param>
    /// <param name="name">The name of the header to retrieve.</param>
    /// <returns>The first value of the specified header, or null if the header is not found.</returns>
    public static string GetValue(this HttpHeaders header, string name)
    {
        if (header.TryGetValues(name, out IEnumerable<string> values))
        {
            return values.FirstOrDefault();
        }
        else
        {
            return null;
        }
    }
}