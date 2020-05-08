namespace NuGet
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;

    internal class AggregateConstraintProvider : IPackageConstraintProvider
    {
        private readonly IEnumerable<IPackageConstraintProvider> _constraintProviders;

        public AggregateConstraintProvider(params IPackageConstraintProvider[] constraintProviders)
        {
            if (constraintProviders.IsEmpty<IPackageConstraintProvider>() || Enumerable.Any<IPackageConstraintProvider>(constraintProviders, cp => ReferenceEquals(cp, null)))
            {
                throw new ArgumentNullException("constraintProviders");
            }
            this._constraintProviders = constraintProviders;
        }

        public IVersionSpec GetConstraint(string packageId) => 
            Enumerable.FirstOrDefault<IVersionSpec>(from cp in this._constraintProviders select cp.GetConstraint(packageId), constraint => constraint != null);

        public string Source =>
            string.Join(", ", (IEnumerable<string>) (from cp in this._constraintProviders select cp.Source));

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly AggregateConstraintProvider.<>c <>9 = new AggregateConstraintProvider.<>c();
            public static Func<IPackageConstraintProvider, bool> <>9__1_0;
            public static Func<IPackageConstraintProvider, string> <>9__3_0;
            public static Func<IVersionSpec, bool> <>9__4_1;

            internal bool <.ctor>b__1_0(IPackageConstraintProvider cp) => 
                ReferenceEquals(cp, null);

            internal string <get_Source>b__3_0(IPackageConstraintProvider cp) => 
                cp.Source;

            internal bool <GetConstraint>b__4_1(IVersionSpec constraint) => 
                (constraint != null);
        }
    }
}

