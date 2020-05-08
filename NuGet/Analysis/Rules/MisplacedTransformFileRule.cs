namespace NuGet.Analysis.Rules
{
    using NuGet;
    using NuGet.Resources;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Runtime.CompilerServices;

    internal class MisplacedTransformFileRule : IPackageRule
    {
        private const string CodeTransformExtension = ".pp";
        private const string ConfigTransformExtension = ".transform";

        private static PackageIssue CreatePackageIssueForMisplacedContent(string path)
        {
            object[] args = new object[] { path };
            return new PackageIssue(AnalysisResources.MisplacedTransformFileTitle, string.Format(CultureInfo.CurrentCulture, AnalysisResources.MisplacedTransformFileDescription, args), AnalysisResources.MisplacedTransformFileSolution);
        }

        [IteratorStateMachine(typeof(<Validate>d__2))]
        public IEnumerable<PackageIssue> Validate(IPackage package)
        {
            <Validate>d__2 d__1 = new <Validate>d__2(-2);
            d__1.<>3__package = package;
            return d__1;
        }

        [CompilerGenerated]
        private sealed class <Validate>d__2 : IEnumerable<PackageIssue>, IEnumerable, IEnumerator<PackageIssue>, IDisposable, IEnumerator
        {
            private int <>1__state;
            private PackageIssue <>2__current;
            private int <>l__initialThreadId;
            private IPackage package;
            public IPackage <>3__package;
            private IEnumerator<IPackageFile> <>7__wrap1;

            [DebuggerHidden]
            public <Validate>d__2(int <>1__state)
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
                    }
                    else if (num == 1)
                    {
                        this.<>1__state = -3;
                    }
                    else
                    {
                        return false;
                    }
                    while (true)
                    {
                        if (!this.<>7__wrap1.MoveNext())
                        {
                            this.<>m__Finally1();
                            this.<>7__wrap1 = null;
                            flag = false;
                        }
                        else
                        {
                            string path = this.<>7__wrap1.Current.Path;
                            if (!path.EndsWith(".pp", StringComparison.OrdinalIgnoreCase) && !path.EndsWith(".transform", StringComparison.OrdinalIgnoreCase))
                            {
                                continue;
                            }
                            if (path.StartsWith(Constants.ContentDirectory + Path.DirectorySeparatorChar.ToString(), StringComparison.OrdinalIgnoreCase))
                            {
                                continue;
                            }
                            this.<>2__current = MisplacedTransformFileRule.CreatePackageIssueForMisplacedContent(path);
                            this.<>1__state = 1;
                            flag = true;
                        }
                        break;
                    }
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
                MisplacedTransformFileRule.<Validate>d__2 d__;
                if ((this.<>1__state != -2) || (this.<>l__initialThreadId != Environment.CurrentManagedThreadId))
                {
                    d__ = new MisplacedTransformFileRule.<Validate>d__2(0);
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

