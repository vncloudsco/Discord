namespace NuGet
{
    using System.Net;

    internal interface IShimWebRequest
    {
        HttpWebRequest Request { get; }
    }
}

