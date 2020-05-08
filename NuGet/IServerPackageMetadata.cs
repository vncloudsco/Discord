namespace NuGet
{
    using System;

    internal interface IServerPackageMetadata
    {
        Uri ReportAbuseUrl { get; }

        int DownloadCount { get; }
    }
}

