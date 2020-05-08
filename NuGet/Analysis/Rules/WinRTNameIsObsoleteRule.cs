namespace NuGet.Analysis.Rules
{
    using NuGet;
    using NuGet.Resources;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.Runtime.CompilerServices;

    internal class WinRTNameIsObsoleteRule : IPackageRule
    {
        private static string[] Prefixes = new string[] { @"content\winrt45\", @"lib\winrt45\", @"tools\winrt45\", @"content\winrt\", @"lib\winrt\", @"tools\winrt\" };

        private static PackageIssue CreateIssue(IPackageFile file)
        {
            object[] args = new object[] { file.Path };
            return new PackageIssue(AnalysisResources.WinRTObsoleteTitle, string.Format(CultureInfo.CurrentCulture, AnalysisResources.WinRTObsoleteDescription, args), AnalysisResources.WinRTObsoleteSolution);
        }

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
            private IPackageFile <file>5__1;
            private IEnumerator<IPackageFile> <>7__wrap1;
            private string[] <>7__wrap2;
            private int <>7__wrap3;

            [DebuggerHidden]
            public <Validate>d__1(int <>1__state)
            {
                this.<>1__state = <>1__state;
                this.<>l__initialThreadId = Environment.CurrentManagedThreadId;
            }

            private void <>m__Finally1()
            {
                this.<>1__state = -1;
                if (this.<>7__wrap1 != null)
                {
                    this.<>7__wrap1.Dispose();
                }
            }

            private bool MoveNext()
            {
                bool flag;
                try
                {
                    int num = this.<>1__state;
                    if (num == 0)
                    {
                        this.<>1__state = -1;
                        this.<>7__wrap1 = this.package.GetFiles().GetEnumerator();
                        this.<>1__state = -3;
                        goto TR_0005;
                    }
                    else if (num == 1)
                    {
                        this.<>1__state = -3;
                        goto TR_000D;
                    }
                    else
                    {
                        flag = false;
                    }
                    return flag;
                TR_0005:
                    if (this.<>7__wrap1.MoveNext())
                    {
                        this.<file>5__1 = this.<>7__wrap1.Current;
                        this.<>7__wrap2 = WinRTNameIsObsoleteRule.Prefixes;
                        this.<>7__wrap3 = 0;
                    }
                    else
                    {
                        this.<>m__Finally1();
                        this.<>7__wrap1 = null;
                        return false;
                    }
                TR_000B:
                    while (true)
                    {
                        if (this.<>7__wrap3 < this.<>7__wrap2.Length)
                        {
                            string str = this.<>7__wrap2[this.<>7__wrap3];
                            if (this.<file>5__1.Path.StartsWith(str, StringComparison.OrdinalIgnoreCase))
                            {
                                this.<>2__current = WinRTNameIsObsoleteRule.CreateIssue(this.<file>5__1);
                                this.<>1__state = 1;
                                flag = true;
                                break;
                            }
                        }
                        else
                        {
                            this.<>7__wrap2 = null;
                            this.<file>5__1 = null;
                            goto TR_0005;
                        }
                        goto TR_000D;
                    }
                    return flag;
                TR_000D:
                    while (true)
                    {
                        this.<>7__wrap3++;
                        break;
                    }
                    goto TR_000B;
                }
                fault
                {
                    this.System.IDisposable.Dispose();
                }
                return flag;
            }

            [DebuggerHidden]
            IEnumerator<PackageIssue> IEnumerable<PackageIssue>.GetEnumerator()
            {
                WinRTNameIsObsoleteRule.<Validate>d__1 d__;
                if ((this.<>1__state != -2) || (this.<>l__initialThreadId != Environment.CurrentManagedThreadId))
                {
                    d__ = new WinRTNameIsObsoleteRule.<Validate>d__1(0);
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
                int num = this.<>1__state;
                if ((num == -3) || (num == 1))
                {
                    try
                    {
                    }
                    finally
                    {
                        this.<>m__Finally1();
                    }
                }
            }

            PackageIssue IEnumerator<PackageIssue>.Current =>
                this.<>2__current;

            object IEnumerator.Current =>
                this.<>2__current;
        }
    }
}

