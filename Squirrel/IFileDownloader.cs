namespace Squirrel
{
    using System;
    using System.Threading.Tasks;

    internal interface IFileDownloader
    {
        Task DownloadFile(string url, string targetFile, Action<int> progress);
        Task<byte[]> DownloadUrl(string url);
    }
}

