namespace NuGet
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Runtime.Versioning;

    internal class PhysicalPackageFile : IPackageFile, IFrameworkTargetable
    {
        private readonly Func<Stream> _streamFactory;
        private string _targetPath;
        private FrameworkName _targetFramework;

        public PhysicalPackageFile()
        {
        }

        public PhysicalPackageFile(PhysicalPackageFile file)
        {
            this.SourcePath = file.SourcePath;
            this.TargetPath = file.TargetPath;
        }

        internal PhysicalPackageFile(Func<Stream> streamFactory)
        {
            this._streamFactory = streamFactory;
        }

        public override bool Equals(object obj)
        {
            PhysicalPackageFile file = obj as PhysicalPackageFile;
            return ((file != null) && (string.Equals(this.SourcePath, file.SourcePath, StringComparison.OrdinalIgnoreCase) && string.Equals(this.TargetPath, file.TargetPath, StringComparison.OrdinalIgnoreCase)));
        }

        public override int GetHashCode()
        {
            int hashCode = 0;
            if (this.SourcePath != null)
            {
                hashCode = this.SourcePath.GetHashCode();
            }
            if (this.TargetPath != null)
            {
                hashCode = (hashCode * 0x11d7) + this.TargetPath.GetHashCode();
            }
            return hashCode;
        }

        public Stream GetStream() => 
            ((this._streamFactory != null) ? this._streamFactory() : File.OpenRead(this.SourcePath));

        public override string ToString() => 
            this.TargetPath;

        public string SourcePath { get; set; }

        public string TargetPath
        {
            get => 
                this._targetPath;
            set
            {
                if (string.Compare(this._targetPath, value, StringComparison.OrdinalIgnoreCase) != 0)
                {
                    string str;
                    this._targetPath = value;
                    this._targetFramework = VersionUtility.ParseFrameworkNameFromFilePath(this._targetPath, out str);
                    this.EffectivePath = str;
                }
            }
        }

        public string Path =>
            this.TargetPath;

        public string EffectivePath { get; private set; }

        public FrameworkName TargetFramework =>
            this._targetFramework;

        public IEnumerable<FrameworkName> SupportedFrameworks
        {
            [IteratorStateMachine(typeof(<get_SupportedFrameworks>d__22))]
            get
            {
                <get_SupportedFrameworks>d__22 d__1 = new <get_SupportedFrameworks>d__22(-2);
                d__1.<>4__this = this;
                return d__1;
            }
        }

        [CompilerGenerated]
        private sealed class <get_SupportedFrameworks>d__22 : IEnumerable<FrameworkName>, IEnumerable, IEnumerator<FrameworkName>, IDisposable, IEnumerator
        {
            private int <>1__state;
            private FrameworkName <>2__current;
            private int <>l__initialThreadId;
            public PhysicalPackageFile <>4__this;

            [DebuggerHidden]
            public <get_SupportedFrameworks>d__22(int <>1__state)
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
                PhysicalPackageFile.<get_SupportedFrameworks>d__22 d__;
                if ((this.<>1__state == -2) && (this.<>l__initialThreadId == Environment.CurrentManagedThreadId))
                {
                    this.<>1__state = 0;
                    d__ = this;
                }
                else
                {
                    d__ = new PhysicalPackageFile.<get_SupportedFrameworks>d__22(0) {
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

