namespace NuGet.Analysis.Rules
{
    using NuGet;
    using NuGet.Resources;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;

    internal class MissingSummaryRule : IPackageRule
    {
        private const int ThresholdDescriptionLength = 300;

        [IteratorStateMachine(typeof(<Validate>d__1))]
        public IEnumerable<PackageIssue> Validate(IPackage package)
        {
            <Validate>d__1 d__1 = new <Validate>d__1(-2);
            d__1.<>3__package = package;
            return d__1;
        }

        [CompilerGenerated]
        private sealed class <Validate>d__1 : IEnumerable<PackageIssue>, IEnumerable, IEnumerator<PackageIssue>, IDisposable, IEnumerator
        {
            private int <>1__state;
            private PackageIssue <>2__current;
            private int <>l__initialThreadId;
            private IPackage package;
            public IPackage <>3__package;

            [DebuggerHidden]
            public <Validate>d__1(int <>1__state)
            {
                this.<>1__state = <>1__state;
                this.<>l__initialThreadId = Environment.CurrentManagedThreadId;
            }

            private bool MoveNext()
            {
                int num = this.<>1__state;
                if (num != 0)
                {
                    if (num != 1)
                    {
                        return false;
                    }
                    this.<>1__state = -1;
                }
                else
                {
                    this.<>1__state = -1;
                    if ((this.package.Description.Length > 300) && string.IsNullOrEmpty(this.package.Summary))
                    {
                        this.<>2__current = new PackageIssue(AnalysisResources.MissingSummaryTitle, AnalysisResources.MissingSummaryDescription, AnalysisResources.MissingSummarySolution);
                        this.<>1__state = 1;
                        return true;
                    }
                }
                return false;
            }

            [DebuggerHidden]
            IEnumerator<PackageIssue> IEnumerable<PackageIssue>.GetEnumerator()
            {
                MissingSummaryRule.<Validate>d__1 d__;
                if ((this.<>1__state != -2) || (this.<>l__initialThreadId != Environment.CurrentManagedThreadId))
                {
                    d__ = new MissingSummaryRule.<Validate>d__1(0);
                }
                else
                {
                    this.<>1__state = 0;
                    d__ = this;
                }
                d__.package = this.<>3__package;
                return d__;
            }

            [DebuggerHidden]
            IEnumerator IEnumerable.GetEnumerator() => 
                this.System.Collections.Generic.IEnumerable<NuGet.PackageIssue>.GetEnumerator();

            [DebuggerHidden]
            void IEnumerator.Reset()
            {
                throw new NotSupportedException();
            }

            [DebuggerHidden]
            void IDisposable.Dispose()
            {
            }

            PackageIssue IEnumerator<PackageIssue>.Current =>
                this.<>2__current;

            object IEnumerator.Current =>
                this.<>2__current;
        }
    }
}

