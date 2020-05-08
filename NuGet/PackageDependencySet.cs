namespace NuGet
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Runtime.Versioning;

    internal class PackageDependencySet : IFrameworkTargetable
    {
        private readonly FrameworkName _targetFramework;
        private readonly ReadOnlyCollection<PackageDependency> _dependencies;

        public PackageDependencySet(FrameworkName targetFramework, IEnumerable<PackageDependency> dependencies)
        {
            if (dependencies == null)
            {
                throw new ArgumentNullException("dependencies");
            }
            this._targetFramework = targetFramework;
            this._dependencies = new ReadOnlyCollection<PackageDependency>(dependencies.ToList<PackageDependency>());
        }

        public FrameworkName TargetFramework =>
            this._targetFramework;

        public ICollection<PackageDependency> Dependencies =>
            this._dependencies;

        public IEnumerable<FrameworkName> SupportedFrameworks
        {
            [IteratorStateMachine(typeof(<get_SupportedFrameworks>d__8))]
            get
            {
                <get_SupportedFrameworks>d__8 d__1 = new <get_SupportedFrameworks>d__8(-2);
                d__1.<>4__this = this;
                return d__1;
            }
        }

        [CompilerGenerated]
        private sealed class <get_SupportedFrameworks>d__8 : IEnumerable<FrameworkName>, IEnumerable, IEnumerator<FrameworkName>, IDisposable, IEnumerator
        {
            private int <>1__state;
            private FrameworkName <>2__current;
            private int <>l__initialThreadId;
            public PackageDependencySet <>4__this;

            [DebuggerHidden]
            public <get_SupportedFrameworks>d__8(int <>1__state)
            {
                this.<>1__state = <>1__state;
                this.<>l__initialThreadId = Environment.CurrentManagedThreadId;
            }

            private bool MoveNext()
            {
                int num = this.<>1__state;
                if (num != 0)
                {
                    if (num == 1)
                    {
                        this.<>1__state = -1;
                    }
                    return false;
                }
                this.<>1__state = -1;
                if (this.<>4__this.TargetFramework == null)
                {
                    return false;
                }
                this.<>2__current = this.<>4__this.TargetFramework;
                this.<>1__state = 1;
                return true;
            }

            [DebuggerHidden]
            IEnumerator<FrameworkName> IEnumerable<FrameworkName>.GetEnumerator()
            {
                PackageDependencySet.<get_SupportedFrameworks>d__8 d__;
                if ((this.<>1__state == -2) && (this.<>l__initialThreadId == Environment.CurrentManagedThreadId))
                {
                    this.<>1__state = 0;
                    d__ = this;
                }
                else
                {
                    d__ = new PackageDependencySet.<get_SupportedFrameworks>d__8(0) {
                        <>4__this = this.<>4__this
                    };
                }
                return d__;
            }

            [DebuggerHidden]
            IEnumerator IEnumerable.GetEnumerator() => 
                this.System.Collections.Generic.IEnumerable<System.Runtime.Versioning.FrameworkName>.GetEnumerator();

            [DebuggerHidden]
            void IEnumerator.Reset()
            {
                throw new NotSupportedException();
            }

            [DebuggerHidden]
            void IDisposable.Dispose()
            {
            }

            FrameworkName IEnumerator<FrameworkName>.Current =>
                this.<>2__current;

            object IEnumerator.Current =>
                this.<>2__current;
        }
    }
}

