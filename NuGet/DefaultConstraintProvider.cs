namespace NuGet
{
    using System;
    using System.Collections.Generic;

    internal class DefaultConstraintProvider : IPackageConstraintProvider
    {
        private readonly Dictionary<string, IVersionSpec> _constraints = new Dictionary<string, IVersionSpec>(StringComparer.OrdinalIgnoreCase);

        public void AddConstraint(string packageId, IVersionSpec versionSpec)
        {
            this._constraints[packageId] = versionSpec;
        }

        public IVersionSpec GetConstraint(string packageId)
        {
            IVersionSpec spec;
            return (!this._constraints.TryGetValue(packageId, out spec) ? null : spec);
        }

        public string Source =>
            string.Empty;
    }
}

