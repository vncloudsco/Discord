namespace NuGet
{
    using System;
    using System.Runtime.CompilerServices;

    internal class NullPropertyProvider : IPropertyProvider
    {
        private static readonly NullPropertyProvider _instance = new NullPropertyProvider();

        private NullPropertyProvider()
        {
        }

        [return: Dynamic]
        public object GetPropertyValue(string propertyName) => 
            null;

        public static NullPropertyProvider Instance =>
            _instance;
    }
}

