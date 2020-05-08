namespace NuGet
{
    using NuGet.Resources;
    using System;
    using System.Globalization;
    using System.Text.RegularExpressions;

    internal static class PackageIdValidator
    {
        internal const int MaxPackageIdLength = 100;
        private static readonly Regex _idRegex = new Regex(@"^\w+([_.-]\w+)*$", RegexOptions.ExplicitCapture | RegexOptions.IgnoreCase);

        public static bool IsValidPackageId(string packageId)
        {
            if (packageId == null)
            {
                throw new ArgumentNullException("packageId");
            }
            return _idRegex.IsMatch(packageId);
        }

        public static void ValidatePackageId(string packageId)
        {
            if (packageId.Length > 100)
            {
                throw new ArgumentException(NuGetResources.Manifest_IdMaxLengthExceeded);
            }
            if (!IsValidPackageId(packageId))
            {
                object[] args = new object[] { packageId };
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, NuGetResources.InvalidPackageId, args));
            }
        }
    }
}

