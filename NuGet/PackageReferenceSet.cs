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

    internal class PackageReferenceSet : IFrameworkTargetable
    {
        private readonly FrameworkName _targetFramework;
        private readonly ICollection<string> _references;

        public PackageReferenceSet(ManifestReferenceSet manifestReferenceSet)
        {
            if (manifestReferenceSet == null)
            {
                throw new ArgumentNullException("manifestReferenceSet");
            }
            if (!string.IsNullOrEmpty(manifestReferenceSet.TargetFramework))
            {
                this._targetFramework = VersionUtility.ParseFrameworkName(manifestReferenceSet.TargetFramework);
            }
            this._references = new ReadOnlyHashSet<string>(from r in manifestReferenceSet.References select r.File, StringComparer.OrdinalIgnoreCase);
        }

        public PackageReferenceSet(FrameworkName targetFramework, IEnumerable<string> references)
        {
            if (references == null)
            {
                throw new ArgumentNullException("references");
            }
            this._targetFramework = targetFramework;
            this._references = new ReadOnlyCollection<string>(references.ToList<string>());
        }

        public ICollection<string> References =>
            this._references;

        public FrameworkName TargetFramework =>
            this._targetFramework;

        public IEnumerable<FrameworkName> SupportedFrameworks
        {
            [IteratorStateMachine(typeof(<get_SupportedFrameworks>d__9))]
            get
            {
                <get_SupportedFrameworks>d__9 d__1 = new <get_SupportedFrameworks>d__9(-2);
                d__1.<>4__this = this;
                return d__1;
            }
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly PackageReferenceSet.<>c <>9 = new PackageReferenceSet.<>c();
            public static Func<ManifestReference, string> <>9__3_0;

            internal string <.ctor>b__3_0(ManifestReference r) => 
                r.File;
        }

        [CompilerGenerated]
        private sealed class <get_SupportedFrameworks>d__9 : IEnumerable<FrameworkName>, IEnumerable, IEnumerator<FrameworkName>, IDisposable, IEnumerator
        {
            private int <>1__state;
            private FrameworkName <>2__current;
            private int <>l__initialThreadId;
            public PackageReferenceSet <>4__this;

            [DebuggerHidden]
            public <get_SupportedFrameworks>d__9(int <>1__state)
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
                PackageReferenceSet.<get_SupportedFrameworks>d__9 d__;
                if ((this.<>1__state == -2) && (this.<>l__initialThreadId == Environment.CurrentManagedThreadId))
                {
                    this.<>1__state = 0;
                    d__ = this;
                }
                else
                {
                    d__ = new PackageReferenceSet.<get_SupportedFrameworks>d__9(0) {
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

