namespace NuGet
{
    internal interface ICloneableRepository
    {
        IPackageRepository Clone();
    }
}

