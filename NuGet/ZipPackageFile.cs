namespace NuGet
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.IO.Packaging;
    using System.Runtime.CompilerServices;
    using System.Runtime.Versioning;

    internal class ZipPackageFile : IPackageFile, IFrameworkTargetable
    {
        private readonly Func<Stream> _streamFactory;
        private readonly FrameworkName _targetFramework;

        public ZipPackageFile(IPackageFile file) : this(file.Path, file.GetStream().ToStreamFactory())
        {
        }

        public ZipPackageFile(PackagePart part) : this(UriUtility.GetPath(part.Uri), part.GetStream().ToStreamFactory())
        {
        }

        protected ZipPackageFile(string path, Func<Stream> streamFactory)
        {
            string str;
            this.Path = path;
            this._streamFactory = streamFactory;
            this._targetFramework = VersionUtility.ParseFrameworkNameFromFilePath(path, out str);
            this.EffectivePath = str;
        }

        public Stream GetStream() => 
            this._streamFactory();

        public override string ToString() => 
            this.Path;

        public string Path { get; private set; }

        public string EffectivePath { get; private set; }

        public FrameworkName TargetFramework =>
            this._targetFramework;

        IEnumerable<FrameworkName> IFrameworkTargetable.SupportedFrameworks
        {
            [IteratorStateMachine(typeof(<NuGet-IFrameworkTargetable-get_SupportedFrameworks>d__16))]
            get
            {
                <NuGet-IFrameworkTargetable-get_SupportedFrameworks>d__16 d__1 = new <NuGet-IFrameworkTargetable-get_SupportedFrameworks>d__16(-2);
                d__1.<>4__this = this;
                return d__1;
            }
        }

        [CompilerGenerated]
        private sealed class <NuGet-IFrameworkTargetable-get_SupportedFrameworks>d__16 : IEnumerable<FrameworkName>, IEnumerable, IEnumerator<FrameworkName>, IDisposable, IEnumerator
        {
            private int <>1__state;
            private FrameworkName <>2__current;
            private int <>l__initialThreadId;
            public ZipPackageFile <>4__this;

            [DebuggerHidden]
            public <NuGet-IFrameworkTargetable-get_SupportedFrameworks>d__16(int <>1__state)
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
                    if (this.<>4__this.TargetFramework != null)
                    {
                        this.<>2__current = this.<>4__this.TargetFramework;
                        this.<>1__state = 1;
                        return true;
                    }
                }
                return false;
            }

            [DebuggerHidden]
            IEnumerator<FrameworkName> IEnumerable<FrameworkName>.GetEnumerator()
            {
                ZipPackageFile.<NuGet-IFrameworkTargetable-get_SupportedFrameworks>d__16 d__;
                if ((this.<>1__state == -2) && (this.<>l__initialThreadId == Environment.CurrentManagedThreadId))
                {
                    this.<>1__state = 0;
                    d__ = this;
                }
                else
                {
                    d__ = new ZipPackageFile.<NuGet-IFrameworkTargetable-get_SupportedFrameworks>d__16(0) {
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

