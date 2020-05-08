namespace NuGet
{
    using System;

    internal interface IOperationAwareRepository
    {
        IDisposable StartOperation(string operation, string mainPackageId, string mainPackageVersion);
    }
}

