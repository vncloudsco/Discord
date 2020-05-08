namespace NuGet
{
    using System.Globalization;

    internal interface ICultureAwareRepository
    {
        CultureInfo Culture { get; }
    }
}

