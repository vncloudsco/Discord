namespace NuGet
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Runtime.CompilerServices;

    internal class FrameworkAssemblyReference : IFrameworkTargetable
    {
        public FrameworkAssemblyReference(string assemblyName) : this(assemblyName, Enumerable.Empty<FrameworkName>())
        {
        }

        public FrameworkAssemblyReference(string assemblyName, IEnumerable<FrameworkName> supportedFrameworks)
        {
            if (string.IsNullOrEmpty(assemblyName))
            {
                object[] args = new object[] { "assemblyName" };
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, CommonResources.Argument_Cannot_Be_Null_Or_Empty, args));
            }
            if (supportedFrameworks == null)
            {
                throw new ArgumentNullException("supportedFrameworks");
            }
            this.AssemblyName = assemblyName;
            this.SupportedFrameworks = supportedFrameworks;
        }

        public string AssemblyName { get; private set; }

        public IEnumerable<FrameworkName> SupportedFrameworks { get; private set; }
    }
}

