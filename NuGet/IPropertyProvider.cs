namespace NuGet
{
    using System;
    using System.Runtime.CompilerServices;

    internal interface IPropertyProvider
    {
        [return: Dynamic]
        object GetPropertyValue(string propertyName);
    }
}

