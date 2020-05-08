namespace NuGet
{
    using System;

    internal interface IEnvironmentVariableReader
    {
        string GetEnvironmentVariable(string variable);
    }
}

