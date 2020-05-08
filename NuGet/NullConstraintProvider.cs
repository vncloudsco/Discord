namespace NuGet
{
    using System;

    internal class NullConstraintProvider : IPackageConstraintProvider
    {
        private static readonly NullConstraintProvider _instance = new NullConstraintProvider();

        private NullConstraintProvider()
        {
        }

        public IVersionSpec GetConstraint(string packageId) => 
            null;

        public static NullConstraintProvider Instance =>
            _instance;

        public string Source =>
            string.Empty;
    }
}

