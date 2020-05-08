namespace NuGet
{
    using System;
    using System.Security;

    internal class EnvironmentVariableWrapper : IEnvironmentVariableReader
    {
        public string GetEnvironmentVariable(string variable)
        {
            try
            {
                return Environment.GetEnvironmentVariable(variable);
            }
            catch (SecurityException)
            {
                return null;
            }
        }
    }
}

