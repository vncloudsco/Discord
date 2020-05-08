namespace NuGet
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Packaging;
    using System.Linq;

    internal static class UriUtility
    {
        private static Uri CreateODataAgnosticUri(string uri)
        {
            if (uri.EndsWith("$metadata", StringComparison.OrdinalIgnoreCase))
            {
                char[] trimChars = new char[] { '/' };
                uri = uri.Substring(0, uri.Length - 9).TrimEnd(trimChars);
            }
            return new Uri(uri);
        }

        internal static Uri CreatePartUri(string path)
        {
            char[] separator = new char[] { '/', Path.DirectorySeparatorChar };
            IEnumerable<string> values = Enumerable.Select<string, string>(path.Split(separator, StringSplitOptions.None), new Func<string, string>(Uri.EscapeDataString));
            return PackUriHelper.CreatePartUri(new Uri(string.Join("/", values), UriKind.Relative));
        }

        internal static string GetPath(Uri uri)
        {
            string originalString = uri.OriginalString;
            if (originalString.StartsWith("/", StringComparison.Ordinal))
            {
                originalString = originalString.Substring(1);
            }
            return Uri.UnescapeDataString(originalString.Replace('/', Path.DirectorySeparatorChar));
        }

        public static bool UriEquals(Uri uri1, Uri uri2)
        {
            char[] trimChars = new char[] { '/' };
            uri1 = CreateODataAgnosticUri(uri1.OriginalString.TrimEnd(trimChars));
            char[] chArray2 = new char[] { '/' };
            uri2 = CreateODataAgnosticUri(uri2.OriginalString.TrimEnd(chArray2));
            return (Uri.Compare(uri1, uri2, UriComponents.Path | UriComponents.SchemeAndServer, UriFormat.SafeUnescaped, StringComparison.OrdinalIgnoreCase) == 0);
        }

        public static bool UriStartsWith(Uri uri1, Uri uri2) => 
            (UriEquals(uri1, uri2) || uri1.IsBaseOf(uri2));
    }
}

